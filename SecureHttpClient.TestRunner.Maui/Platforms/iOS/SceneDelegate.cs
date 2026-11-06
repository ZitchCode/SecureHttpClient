using Foundation;
using Microsoft.Maui;

namespace SecureHttpClient.TestRunner.Maui
{
    // Xcode 27 (iOS 27 SDK) fails at launch when the app does not adopt the UIKit scene
    // lifecycle. MauiUISceneDelegate creates the MAUI window in WillConnect and forwards
    // scene callbacks to the cross-platform lifecycle, so the class stays empty.
    // The registered name must match UISceneDelegateClassName in Info.plist.
    [Register("SceneDelegate")]
    public class SceneDelegate : MauiUISceneDelegate
    {
    }
}
