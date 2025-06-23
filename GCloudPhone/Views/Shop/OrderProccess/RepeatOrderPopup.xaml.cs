using CommunityToolkit.Maui.Views;

namespace GCloudPhone.Views.Shop.OrderProcesses;

public partial class RepeatOrderPopup : Popup
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; private set; }
    public RepeatOrderPopup()
	{
		InitializeComponent();
        TaskCompletionSource = new TaskCompletionSource<bool>();
    }

    private void OnYesClicked(object sender, EventArgs e)
    {
        TaskCompletionSource.SetResult(true);
        Close();
    }

    private void OnNoClicked(object sender, EventArgs e)
    {
        TaskCompletionSource.SetResult(false);
        Close();
    }
}