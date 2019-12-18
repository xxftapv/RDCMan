using AxMSTSCLib;
using System.ComponentModel;

namespace RdcMan
{
	internal class RdpClient8 : AxMsRdpClient8NotSafeForScripting, IRdpClient
	{
		public RdpClient8(MainForm form)
		{
			((ISupportInitialize)this).BeginInit();
			Hide();
			form.AddToClientPanel(this);
			((ISupportInitialize)this).EndInit();
		}
	}
}