using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Audio_Pipes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();

            if (Microsoft.Services.Store.Engagement.Feedback.IsSupported)
            {
                Feedback.Visibility = Visibility.Visible;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void Feedback_Click(object sender, RoutedEventArgs e)
        {
            await Microsoft.Services.Store.Engagement.Feedback.LaunchFeedbackAsync();
        }

        private async void DonateSmall_Click(object sender, RoutedEventArgs e)
        {
            //await CurrentAppSimulator.ReloadSimulatorAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///WindowsStoreProxy.xml")));
            try
            {
                PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync("DonateSmall");
            
    
                Guid transactionId = new Guid();

                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        transactionId = purchaseResults.TransactionId;

                        // Grant the user their purchase here, and then pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;

                    case ProductPurchaseStatus.NotFulfilled:
                        transactionId = purchaseResults.TransactionId;

                        // First check for unfulfilled purchases and grant any unfulfilled purchases from an earlier transaction.
                        // Once products are fulfilled pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;
                }

                FulfillmentResult result = await CurrentApp.ReportConsumableFulfillmentAsync("DonateSmall", transactionId);

                if (purchaseResults.Status != ProductPurchaseStatus.NotPurchased)
                {
                    await new Windows.UI.Popups.MessageDialog("Thank you for your donation.  It really is appreciated.").ShowAsync();
                }
            }
            catch
            {
                await new Windows.UI.Popups.MessageDialog("Unable to process transaction.  Please try again later.").ShowAsync();
            }
        }

        private async void DonateMedium_Click(object sender, RoutedEventArgs e)
        {
            //await CurrentAppSimulator.ReloadSimulatorAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///WindowsStoreProxy.xml")));
            try
            {
                PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync("DonateMedium");


                Guid transactionId = new Guid();

                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        transactionId = purchaseResults.TransactionId;

                        // Grant the user their purchase here, and then pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;

                    case ProductPurchaseStatus.NotFulfilled:
                        transactionId = purchaseResults.TransactionId;

                        // First check for unfulfilled purchases and grant any unfulfilled purchases from an earlier transaction.
                        // Once products are fulfilled pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;
                }

                FulfillmentResult result = await CurrentApp.ReportConsumableFulfillmentAsync("DonateMedium", transactionId);

                if (purchaseResults.Status != ProductPurchaseStatus.NotPurchased)
                {
                    await new Windows.UI.Popups.MessageDialog("Thank you for your donation.  It really is appreciated.").ShowAsync();
                }
            }
            catch
            {
                await new Windows.UI.Popups.MessageDialog("Unable to process transaction.  Please try again later.").ShowAsync();
            }
        }

        private async void DonateLarge_Click(object sender, RoutedEventArgs e)
        {
            //await CurrentAppSimulator.ReloadSimulatorAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///WindowsStoreProxy.xml")));
            try
            {
                PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync("DonateLarge");


                Guid transactionId = new Guid();

                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        transactionId = purchaseResults.TransactionId;

                        // Grant the user their purchase here, and then pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;

                    case ProductPurchaseStatus.NotFulfilled:
                        transactionId = purchaseResults.TransactionId;

                        // First check for unfulfilled purchases and grant any unfulfilled purchases from an earlier transaction.
                        // Once products are fulfilled pass the product ID and transaction ID to currentAppSimulator.reportConsumableFulfillment
                        // To indicate local fulfillment to the Windows Store.
                        break;
                }

                FulfillmentResult result = await CurrentApp.ReportConsumableFulfillmentAsync("DonateLarge", transactionId);

                if (purchaseResults.Status != ProductPurchaseStatus.NotPurchased)
                {
                    await new Windows.UI.Popups.MessageDialog("Thank you for your donation.  It really is appreciated.").ShowAsync();
                }
            }
            catch
            {
                await new Windows.UI.Popups.MessageDialog("Unable to process transaction.  Please try again later.").ShowAsync();
            }
        }
    }
}
