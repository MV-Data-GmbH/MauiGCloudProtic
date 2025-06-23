using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GCloud.Extensions;
using Newtonsoft.Json;

namespace GCloud.Controllers.ViewModels.Home
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