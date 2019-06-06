package com.uwb.xr.droneGame;

public interface UnityDroneInterface {
    // C# -> JAVA:
    void refreshConnectionStatus();
    void followMeStart();
    void followMeStop();
    void showToast( final String message );
    void setThrottle(float val);
    void disableVideo();
    void enableVideo();
    String getConnectionStatus();
    double[] getDroneAttitude();
    double[] getDroneLocation();
    boolean isDroneConnected();
    int getDroneGpsLevel();
    String getFlightMode();
    String getIMUstate();
    boolean getIsFlying();
    boolean getMotorsOn();
    double[] getPhoneLocation();
    float getPhoneHeading();
    String getProductText();
    String getState();
    byte[] getVideoFrame();
    void land();
    void refreshFlightControllerStatus();
    void setGimbalRotation(float pitchValue, float yawValue);
    void setYaw(float val);
    void setPitch(float val);
    void setRoll(float val);
    void setupDroneConnection();
    void setVirtualControlActive(boolean setting);
    void startLocationService();
    void takeOff();
}
