package com.uwb.xr.droneGame;

import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.os.Debug;
import android.text.TextUtils;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;

import com.unity3d.player.UnityPlayer;

public abstract class UnityDroneActivity extends UnityPlayerActivity implements UnityDroneInterface {

    private static String UNITY_BRIDGE_OBJECT = "DroneCanvas";
    public static final String FRAME_READY = "VIDEO",
                                PHONE_LOC_READY = "PHONE_LOC",
                                DRONE_LOC_READY = "DRONE_LOC",
                                DRONE_ATT_READY = "DRONE_ATT",
                                PHONE_HEAD_READY = "PHONE_HEAD";

    private static final String TAG = UnityDroneActivity.class.getName();

    // Declare overrides to all methods of UnityPlayerActivity
    @Override protected void onCreate(Bundle savedInstanceState) { super.onCreate(savedInstanceState); }
    @Override protected void onNewIntent(Intent intent) { super.onNewIntent(intent); }
    @Override protected void onDestroy () { super.onDestroy(); }
    @Override protected void onPause() { super.onPause();}
    @Override protected void onResume() { super.onResume(); }
    @Override protected void onStart() { super.onStart(); }
    @Override protected void onStop() { super.onStop(); }
    @Override public void onLowMemory() { super.onLowMemory(); }
    @Override public void onTrimMemory(int level) { super.onTrimMemory(level); }
    @Override public void onConfigurationChanged(Configuration newConfig) { super.onConfigurationChanged(newConfig); }
    @Override public void onWindowFocusChanged(boolean hasFocus) { super.onWindowFocusChanged(hasFocus); }
    @Override public boolean dispatchKeyEvent(KeyEvent event)         { return super.dispatchKeyEvent(event); }
    @Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return super.onKeyUp(keyCode, event); }
    @Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return super.onKeyDown(keyCode, event); }
    @Override public boolean onTouchEvent(MotionEvent event)          { return super.onTouchEvent(event); }
    @Override public boolean onGenericMotionEvent(MotionEvent event)  { return super.onGenericMotionEvent(event); }

    // Called by Unity to set the name of the object holding DroneBridge
    public final void registerUnityBridge(String objectName) {
        UNITY_BRIDGE_OBJECT = objectName;
    }

    // JAVA -> C#:

    public final void setReady(String... tags) {
        String param = TextUtils.join(" ", tags);
        UnityPlayer.UnitySendMessage(UNITY_BRIDGE_OBJECT, "SetReady", param);
    }
}
