

namespace GCloudShared.Service.Dto
{
    public class ImageViewModel
    {
       
            public string Name { get; set; }
            public byte[] Image { get; set; }
            public string State => StateEnum.ToString();
            public ImageViewModelState StateEnum { get; set; }
        
    }
    public enum ImageViewModelState
    {
        New, Deleted, UpToDate
    }
}
