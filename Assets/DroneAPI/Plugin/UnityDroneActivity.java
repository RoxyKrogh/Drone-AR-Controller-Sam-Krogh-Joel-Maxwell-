package com.uwb.xr.droneGame;

import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.os.Debug;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;

public class UnityDroneActivity extends UnityPlayerActivity implements UnityDroneInterface {
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


    @Override
    public String getConnectionStatus() {
        Log.v(TAG, "getConnectionStatus()");
        return "TEST_CONNECTION_STATUS";
    }

    @Override
    public double[] getDroneAttitude() {
        Log.v(TAG, "getDroneAttitude()");
        return new double[] { 0, 0, 0 };
    }

    @Override
    public String getProductText() {
        Log.v(TAG, "getProductText()");
        return "TEST_PRODUCT_TEXT";
    }

    @Override
    public String getState() {
        Log.v(TAG, "getState()");
        return "TEST_STATE";
    }

    @Override
    public byte[] getVideoFrame() {
        Log.v(TAG, "getVideoFrame()");
        return new byte[0];
    }

    @Override
    public double[] getPhoneLocation() {
        Log.v(TAG, "getPhoneLocation()");
        return new double[] { 0, 0, 0};
    }

    @Override
    public double[] getDroneLocation() {
        Log.v(TAG, "getDroneLocation()");
        return new double[] { 0, 0, 0};
    }

    @Override
    public String getFlightMode() {
        Log.v(TAG, "getFlightMode()");
        return "TEST_FLIGHT_MODE";
    }

    @Override
    public String getIMUstate() {
        Log.v(TAG, "getIMUstate()");
        return "TEST_IMU_STATE";
    }

    @Override
    public boolean getIsFlying() {
        Log.v(TAG, "getIsFlying()");
        return false;
    }

    @Override
    public boolean getMotorsOn() {
        Log.v(TAG, "getMotorsOn()");
        return false;
    }

    @Override
    public void refreshConnectionStatus() {
        Log.v(TAG, "refreshConnectionStatus()");
    }

    @Override
    public void refreshFlightControllerStatus() {
        Log.v(TAG, "refreshFlightControllerStatus()");
    }

    @Override
    public void setupDroneConnection() {
        Log.v(TAG, "setupDroneConnection()");
    }

    @Override
    public void takeOff() {
        Log.v(TAG, "takeOff()");
    }

    @Override
    public void land() {
        Log.v(TAG, "land()");
    }

    @Override
    public void followMeStart() {
        Log.v(TAG, "followMeStart()");
    }

    @Override
    public void followMeStop() {
        Log.v(TAG, "followMeStop()");
    }

    @Override
    public void startLocationService() {
        Log.v(TAG, "startLocationService()");
    }

    @Override
    public void showToast(String message) {
        Log.v(TAG, "showToast(" + message + ")");
    }

    @Override
    public void setVirtualControlActive(boolean active) {
        Log.v(TAG, "setVirtualControlActive(" + active + ")");
    }

    @Override
    public void setYaw(float val) {
        Log.v(TAG, "setYaw(" + val + ")");
    }

    @Override
    public void setRoll(float val) {
        Log.v(TAG, "setRoll(" + val + ")");
    }

    @Override
    public void setPitch(float val) {
        Log.v(TAG, "setPitch(" + val + ")");
    }

    @Override
    public void setThrottle(float val) {
        Log.v(TAG, "setThrottle(" + val + ")");
    }

    @Override
    public void disableVideo() {
        Log.v(TAG, "disableVideo()");
    }

    @Override
    public void enableVideo() {
        Log.v(TAG, "enableVideo()");
    }

    @Override
    public void setGimbalRotation(float pitchValue, float rollValue) {
        Log.v(TAG, "setGimbalRotation(" + pitchValue + ", " + rollValue + ")");
    }
}
