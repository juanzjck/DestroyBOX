using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;


public class GoogleMobileAdsDemoScript : MonoBehaviour {
    private BannerView bannerView;
  public void Start()
        {
        //ca-app-pub-3940256099942544~3347511713
       // real
        //ca-app-pub-8772213531309625~7038483006
#if UNITY_ANDROID
        string appId = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IPHONE
        string appId = "ca-app-pub-3940256099942544~3347511713";
#else
        string appId = "ca-app-pub-3940256099942544~3347511713";
#endif

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(appId);
        this.RequestBanner();
        }
    private void RequestBanner()
    {

        //ca-app-pub-3940256099942544/6300978111
       // real
        //ca - app - pub - 8772213531309625 / 3646032905
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
    }

}
