using CommunityToolkit.Maui.Views;

namespace GCloudPhone.Views;

public partial class PopupSortList : Popup
{
	public PopupSortList()
	{
		InitializeComponent();
	}
    public static string TypeSort { get; set; }
    public static bool SortASC { get; set; }
    private void Button_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private void radioBtnDatum_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var radio = sender as RadioButton;
        if (radio != null)
        {
            TypeSort = radio.Value.ToString();
        }
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        var imagebutton=sender as ImageButton;
        if (imagebutton != null)
        {
            var cmdparam=Convert.ToInt32(imagebutton.CommandParameter);
            
                if ((int)cmdparam == 0)
                {
                    SortASC= true;
                }
                if ((int)cmdparam == 1)
                {
                    SortASC = false;
                }
            
        }
    }
}