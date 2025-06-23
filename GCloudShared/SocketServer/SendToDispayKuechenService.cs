using GCloudShared.WebShopDto;
using System.Collections.ObjectModel;


namespace GCloudShared.SocketServer
{
    public class SendToDispayKuechenService : NetworkConfiguration
    {
        public ObservableCollection<int> GetService(OrderKuechenDisplay parameter)
        {
            /* if (Status)
                 return null;*/
            Status = true;
            try
            {
                string rw = null;
                try
                {

                    rw = SocketClient.SendMessage(Iad, Iport, "InsertNewWebOrder", 30000, 30000, parameter);
                    if (rw.Contains("Done"))
                    {
                        return new ObservableCollection<int> { 1 };
                    }
                    else
                    {
                        return new ObservableCollection<int> { 0 };
                    }
                   
                }
                catch (Exception ex)
                {
                    var s = ex.Message;
                    Status = false;
                    return null;
                }

               
            }
            catch
            {
                Status = false;
                return null;
            }
  
        }
    }
}
