﻿using Foundation;
using UIKit;
using DittoSDK;
using System;

namespace Tasks
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register ("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate {
    
        [Export("window")]
        public UIWindow Window { get; set; }

        internal Ditto ditto;
        private static DittoPeersObserver peersObserver;

        [Export ("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            DittoLogger.SetLoggingEnabled(true);
            DittoLogger.SetMinimumLogLevel(DittoLogLevel.Verbose);

            NSFileManager fileManager = new NSFileManager();
            NSUrl url = fileManager.GetUrl(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, null, true, out NSError error);
            if (error != null)
            {
                Console.WriteLine($"Error creating Documents directory: {error.LocalizedDescription}");
            }
            url = url.Append("ditto", true);


            fileManager.CreateDirectory(url, true, null, out error);
            if (error != null)
            {
                Console.WriteLine($"Error creating ditto directory: {error.LocalizedDescription}");
            }

            string appId = DittoHandler.Ditto.appId;
            string workingDir = url.Path;

            DittoIdentity identity = DittoIdentity.OfflinePlayground(appID: appId, workingDir: workingDir);

            ditto = new Ditto(identity, workingDir);
            ditto.SetOfflineOnlyLicenseToken(DittoHandler.Ditto.OfflineLicenseToken);

            ditto.DeviceName = UIDevice.CurrentDevice.Name;

            var transportConfig = new DittoTransportConfig();
            transportConfig.EnableAllPeerToPeer();
            Console.WriteLine($"Initial transport config: {transportConfig}");
            ditto.SetTransportConfig(transportConfig);

            peersObserver = ditto.ObservePeers(peers =>
            {
                Console.WriteLine($"Peers connected: {peers.Count}");
                foreach (DittoRemotePeer peer in peers)
                {
                    Console.WriteLine($"Peer: {peer.DeviceName} {peer.NetworkId} {String.Join(", ", peer.Connections)} {peer.Rssi} {peer.ApproximateDistanceInMeters}");
                }
            });

            ditto.TryStartSync();


            return true;
        }

        // UISceneSession Lifecycle

        [Export ("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration (UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            // Called when a new scene session is being created.
            // Use this method to select a configuration to create the new scene with.
            return UISceneConfiguration.Create ("Default Configuration", connectingSceneSession.Role);
        }

        [Export ("application:didDiscardSceneSessions:")]
        public void DidDiscardSceneSessions (UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
            // Called when the user discards a scene session.
            // If any sessions were discarded while the application was not running, this will be called shortly after `FinishedLaunching`.
            // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
        }
    }
}


