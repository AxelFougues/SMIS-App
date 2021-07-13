using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAdScript : MonoBehaviour {

    public string gameId = "4215307";
    public string surfacingId = "bannerPlacement";
    public bool testMode = true;

    void Start() {
        Advertisement.Initialize(gameId, testMode);
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
        StartCoroutine(ShowBannerWhenInitialized());
    }

    IEnumerator ShowBannerWhenInitialized() {
        while (!Advertisement.isInitialized) {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(surfacingId);
    }
}