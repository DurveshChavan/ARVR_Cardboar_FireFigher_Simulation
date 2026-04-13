package com.unity3d.player;

import android.os.Bundle;
import android.util.Log;

/**
 * Custom UnityPlayerActivity that pre-loads the Cardboard native library
 * using the APPLICATION classloader before Unity initializes.
 * 
 * This fixes: "JNI DETECTED ERROR: java_class == null in RegisterNatives"
 * which happens because libGfxPluginCardboard.so's JNI_OnLoad fires with
 * the system bootstrap classloader before Cardboard Java classes are visible.
 *
 * By calling System.loadLibrary() from here, we ensure JNI_OnLoad runs
 * with the correct app classloader so FindClass() can resolve
 * com.google.cardboard.sdk.qrcode.* classes.
 */
public class CardboardUnityPlayerActivity extends UnityPlayerActivity {

    private static final String TAG = "CardboardInit";

    static {
        try {
            System.loadLibrary("GfxPluginCardboard");
            Log.d(TAG, "GfxPluginCardboard loaded successfully from app classloader");
        } catch (UnsatisfiedLinkError e) {
            // Library may already be loaded by Unity — not fatal
            Log.w(TAG, "GfxPluginCardboard pre-load skipped (already loaded or not found): " + e.getMessage());
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.d(TAG, "CardboardUnityPlayerActivity onCreate");
    }
}
