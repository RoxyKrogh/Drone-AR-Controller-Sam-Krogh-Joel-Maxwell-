package com.uwb.xr.djiBridge;

//-------------------------------------
// DJI Backend
// "Here there be dragons"
// Requires no modification by Unity program developer.
// Do so at your own peril!
//-------------------------------------

import android.app.Application;
import android.content.Context;
import android.content.Intent;
import android.graphics.ImageFormat;
import android.graphics.Rect;
import android.graphics.SurfaceTexture;
import android.graphics.YuvImage;
import android.os.AsyncTask;
import android.os.Environment;
import android.os.Handler;
import android.os.Looper;
import android.os.ResultReceiver;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.uwb.xr.droneGame.UnityDroneActivity;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.nio.ByteBuffer;
import java.util.function.Consumer;

import dji.sdk.base.BaseComponent;
import dji.sdk.base.BaseProduct;
import dji.sdk.camera.Camera;
import dji.sdk.camera.VideoFeeder;
import dji.sdk.codec.DJICodecManager;
import dji.sdk.products.Aircraft;
import dji.sdk.products.HandHeld;
import dji.sdk.sdkmanager.DJISDKManager;


public class djiBackend {
    public static final String FLAG_CONNECTION_CHANGE = "activationDemo_connection_change";
    private String TAG = "BACKEND_DRONE";

    // The name of the object within the unity scene that our drone code is attached to
    // TBD: Replace this with a non-final string that's dynamically updated by unity
    private static final String DRONE_OBJ = "DroneCanvas";

    private static final int SURFACE_WIDTH = 16*40, SURFACE_HEIGHT = 9*40;

    // DJI async/callback objects
    private DJISDKManager.SDKManagerCallback mDJISDKManagerCallback;
    private BaseComponent.ComponentListener mDJIComponentListener;

    // Async handler to braodacast connection update messages
    public Handler mHandler;

    // The application context we reference to create the mCodecManager and
    // broadcast intetnts
    private Application mInstance;

    // Instance of the unity player that we send messages to and from
    /* POSSIBLY REMOVABLE, AS USED FUNCTIONS ARE STATIC */
    private UnityPlayer mUnityPlayer;


    // Callback object which triggers whenever there's new video data to decode
    protected VideoFeeder.VideoDataListener mReceivedVideoDataCallBack = null;

    // The video data decoder that turns videofeeder data into usable YUV data
    protected DJICodecManager mCodecManager = null;

    // Tracks whether the most recent video frame is ready to be grabbed
    private Boolean ready;
    // Tracks whether the video processing pipeline should be enabled
    private Boolean video_enabled;

    // After YUV data has been converted to a jpeg image, its data is stored here, and is accessible
    // via getFrameData();
    private byte[] jdata;

    private Consumer<String> onReady;

    public void setOnReadyCallback(Consumer<String> callback) {
        onReady = callback;
    }

    /**
     * Sets this class' local instance of the application context. The context is shared between
     * backend and frontent
     * @param application The application context to be shared
     */
    public void setContext(Application application) {
        mInstance = application;
    }

    /**
     * @return Returns this object's instance of the application context provided by frontend.
     * Null if none provided yet.
     */
    //@Override
    public Context getApplicationContext() {
        return mInstance;
    }

    /**
     * This function is used to get the instance of DJIBaseProduct.
     * If no product is connected, it returns null.
     */
    public static synchronized BaseProduct getProductInstance() {
        return MApplication.getProductInstance();
    }

    /**
     * Provides access to the camera attached to the drone, allowing the use of the camera's
     * photo- and video-taking functionality. All media captures will be stored on the camera itself
     * and can only be downloaded at the cost of interrupting the video stream
     * @return An instance of the camera attached to the drone
     */
    public static synchronized Camera getCameraInstance() {
        return getProductInstance() == null ? null : getProductInstance().getCamera();
    }

    /**
     * Handles the setup of all handlers and callbacks when this object is created
     */
    //@Override
    public void onCreate() {
        Log.d(TAG, "onCreate: DJIBACKEND LOG TESTING");

        // Instantiate our handler to use the looper of the whole application, rather than for
        // the current thread
        mHandler = new Handler(Looper.getMainLooper());

        // Setup our component listener to send a notifystatuschange whenever we change which
        // piece of hardware we're connected to
        mDJIComponentListener = new BaseComponent.ComponentListener() {
            @Override
            public void onConnectivityChange(boolean isConnected) {
                notifyStatusChange();
            }

        };


        // Instantiate our video data decoder
        mCodecManager = new DJICodecManager(mInstance, (SurfaceTexture) null, SURFACE_WIDTH, SURFACE_HEIGHT);

        // We don't want anything to be sent in for processing until we finish setting everything up
        video_enabled = false;

        // Enables the output of YUV data so we can setup the callback in the next line
        mCodecManager.enabledYuvData(true);

        // Setup what should happen whenever our CodecManager finishes processing raw video data
        // into a YUV frame
        mCodecManager.setYuvDataCallback(new DJICodecManager.YuvDataCallback() {
            /**
             * Whenever a frame of decoded YUV data is received, if no frame is currently being
             * processed, the video stream is enabled, and the data isn't too large to allocate an
             * array of bytes for, we ship it off to be processed into a jpeg
             * @param yuvFrame The byte array of YUV data we receive
             * @param dataSize The size of our byte buffer
             * @param width The width of the frame
             * @param height The height of the gram
             */
            @Override
            public void onYuvDataReceived(ByteBuffer yuvFrame, int dataSize, int width, int height) {
                // If a frame is currently being processed or the video stream isn't enabled,
                // we shouldn't process the YUV data
                if(!ready || !video_enabled){
                    Log.d(TAG, "onYuvDataReceived: VIDEO READY:"+ready+" VIDEO ENABLED:"+video_enabled);
                    return;
                }

                // This is an extremely primitive lock that intends to prevent future receipts of
                // YUV data from triggering the pipeline until the current package finishes
                // processing
                ready = false; // Should likely be replaced with a separate variable lock

                //Log.d(TAG, "onYuvDataReceived: DATA SIZE: "+ dataSize + " Width: "+width+ " Height " + height);

                // Our array to hold the buffered YUV data
                byte[] dat;

                try{
                    dat = new byte[dataSize];
                }catch (Exception e){
                    // If somehow the data size is too large to allocate an array for, abandon ship
                    // Note: This doesn't reset ready, but that's likely because if there's an issue
                    // with allocating memory, further YUV data will cause the same issue.
                    Log.e(TAG, "onYuvDataReceived: BYTE failed to allocate.");
                    return;
                }

                // Transfer the YUV data from the buffer to our array
                yuvFrame.get(dat);

                // Async tasks take an array of a single type as their argument, so we need to
                // transfer our YUV frame and our surface width/height to a "struct" containing
                // the three so that we can ship off the data to be asynchronously processed
                Video_Frame_Data[] frameToProcess = {new Video_Frame_Data(dat, width, height)};
                new processFrame().execute(frameToProcess);

            }
        });


        // Setup what should happen whenever our videoFeeder sends us a new frame of raw video data
        mReceivedVideoDataCallBack = new VideoFeeder.VideoDataListener() {
            /**
             * Whenever we receive a new frame of video data from the drone, if a frame of data
             * isn't already being processed, we ship it off to the mCodecManager to be decoded into
             * a YUV frame
             * @param videoBuffer The video data to be processed
             * @param size The size of the video data
             */
            @Override
            public void onReceive(byte[] videoBuffer, int size) {
                // Drop the frame if a frame is already being processed
                if(!ready){
                    Log.d(TAG, "onReceive: VIDEO BUFFER DROPPED.  ready == false");
                    return;
                }

                if (mCodecManager != null) {
                    // Ship off our data to be processed by the codec manager into a YUV frame
                    // When it finishes decoding it, onYuvDataReceived will be called
                    mCodecManager.sendDataToDecoder(videoBuffer, size);
                    Log.d(TAG, "onReceive: VIDEO SENT: " + size);
                } else {
                    Log.d(TAG, "onReceive: CODEC MANAGER NULL");
                }

            }
        };

        // With everything setup, we can now start received video streaming data from the
        // videofeeder
        ready = true;
    }

    /**
     * Provides the backend with an instance of the unity player to use
     * Note: This function may be removable, as the unityplayer functions currently used are all
     * static
     * @param player An instance of the unityplayer, provided by DJIFrontEnd
     */
    public void setUnityObject(UnityPlayer player){
        mUnityPlayer = player;
    }

    /**
     * @return Returns the most recently-generated jpeg from the video data pipeline
     */
    public byte[] getJdata() {
        // Synchronized because ????
        synchronized (this){
            return jdata;
        }
    }

    /**
     * Enables the video processing pipeline
     */
    public void enableVideo(){
        video_enabled = true;
        // Note: This line may be problematic, as 'ready' is intended to be false while video data
        // is being processed, as then only one batch of data can be processed at a time. If
        // it becomes enabled while a batch is being processed, further batches may be passed in
        // for processing accidentally, which could cause further batches to be allowed in.
        ready = true;
    }

    /**
     * Disables the video processing pipeline - data will be received, but not processesd
     */
    public void disableVideo(){
        video_enabled = false;
        // Note: This line may not actually do anything, as any batch in the middle of processesing
        // will cause ready to be false anyway. The only way this works is if it's called between
        // the end of one batch and the beginning of another being received.
        ready = false;
    }


    /* Connection Status Update Functions */

    /**
     * Tells the handler to run our updateRunnable, after first removing all callbacks
     */
    private void notifyStatusChange() {
        mHandler.removeCallbacks(updateRunnable);
        mHandler.postDelayed(updateRunnable, 500);
    }

    /**
     * Broadcasts a message to the frontend notifying it that the connection to the drone has been
     * updated
     */
    private Runnable updateRunnable = new Runnable() {

        @Override
        public void run() {
            Intent intent = new Intent(FLAG_CONNECTION_CHANGE);
            mInstance.sendBroadcast(intent);
        }
    };

    /* Video functions */

    /**
     * Struct-like class that merges all data relevant to a video frame into a single object
     *
     * This is used in the async processFrame task to act as a single-object intput for multiple
     * relevant data points
     */
    class Video_Frame_Data{
        public byte[] frame;
        int frame_width;
        int frame_height;
        public  Video_Frame_Data(byte[] vid_frame, int width, int height){
            frame = vid_frame;
            frame_width = width;
            frame_height = height;
        }
    }

    /**
     * This is where the magic happens. YUV video frames are passed in, and the resulting jpeg
     * is saved in jdata
     */
    class processFrame extends AsyncTask<Video_Frame_Data,Void, byte[]> {
        /**
         * Processes an array of YUV frame data (Always just one frame) to turn it into an
         * RGB jpeg
         * @param data The single-object array containing the frame to process
         * @return Returns the resulting byte array of the image, though the return value isn't
         * actually used anywhere
         */
        @Override
        protected byte[] doInBackground(Video_Frame_Data... data) {
            // Everything after this point, up to the next dashed line, is a bit of a mystery
            // Look up "Convert YUV to RGB" on google to see if you can figure it out
            //----------
            byte[] dat = data[0].frame;
            int height = data[0].frame_height;
            int width = data[0].frame_width;
            byte[] y = new byte[width * height];
            byte[] u = new byte[width * height / 4];
            byte[] v = new byte[width * height / 4];
            byte[] nu = new byte[width * height / 4]; //
            byte[] nv = new byte[width * height / 4];
            System.arraycopy(dat, 0, y, 0, y.length);
            for (int i = 0; i < u.length; i++) {
                v[i] = dat[y.length + 2 * i];
                u[i] = dat[y.length + 2 * i + 1];
            }
            int uvWidth = width / 2;
            int uvHeight = height / 2;
            for (int j = 0; j < uvWidth / 2; j++) {
                for (int i = 0; i < uvHeight / 2; i++) {
                    byte uSample1 = u[i * uvWidth + j];
                    byte uSample2 = u[i * uvWidth + j + uvWidth / 2];
                    byte vSample1 = v[(i + uvHeight / 2) * uvWidth + j];
                    byte vSample2 = v[(i + uvHeight / 2) * uvWidth + j + uvWidth / 2];
                    nu[2 * (i * uvWidth + j)] = uSample1;
                    nu[2 * (i * uvWidth + j) + 1] = uSample1;
                    nu[2 * (i * uvWidth + j) + uvWidth] = uSample2;
                    nu[2 * (i * uvWidth + j) + 1 + uvWidth] = uSample2;
                    nv[2 * (i * uvWidth + j)] = vSample1;
                    nv[2 * (i * uvWidth + j) + 1] = vSample1;
                    nv[2 * (i * uvWidth + j) + uvWidth] = vSample2;
                    nv[2 * (i * uvWidth + j) + 1 + uvWidth] = vSample2;
                }
            }
            byte[] bytes = new byte[dat.length];
            System.arraycopy(y, 0, bytes, 0, y.length);
            for (int i = 0; i < u.length; i++) {
                bytes[y.length + (i * 2)] = nv[i];
                bytes[y.length + (i * 2) + 1] = nu[i];
            }

            //Log.d(TAG, "onYuvDataReceived: BYTE GET");
            //----------

            // Once we have our converted data, we need to convert it to an image
            YuvImage yuvimage = new YuvImage(bytes, ImageFormat.NV21, width, height, null);
            //Log.d(TAG, "onYuvDataReceived: YUVIMAGE created");

            // We'll use this to store the result of our jpeg conversion
            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            // Then, we can convert that image into a jpeg, storing the resulting byte array in our
            // baos
            yuvimage.compressToJpeg(new Rect(0, 0, width, height), 80, baos);
            //Log.d(TAG, "onYuvDataReceived: compress");

            // Finally, we store our byte array in our synchronized jdata object, let our
            // unityplayer object know a new frame is available, and let the pipeline know that
            // another frame can now be pushed through
            jdata = baos.toByteArray();
            onReady.accept(UnityDroneActivity.FRAME_READY); // call onReady in frontend
            ready = true;

            return jdata;
        }
    }
}