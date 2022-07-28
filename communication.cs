using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class communication
{
	private class BulbState
	{
		public TransitionLightState transition_light_state { get; set; } = new TransitionLightState();
	}

	/// <summary>
	/// Do NOT change the name of this class as it is vital for the 
	/// smart bulb to understand it.
	/// </summary>
	private class TransitionLightState
	{
		/// <summary>
		/// 1 = On; 0 = Off
		/// </summary>
		public byte on_off { get; set; } = 1;
		/// <summary>
		/// Transition time from one state to the next in milliseconds
		/// </summary>
		public int transition_period { get; set; } = 2000;
		public int hue { get; set; } = 0;
		public int saturation { get; set; } = 0;
		public int color_temp { get; set; } = 0;
		/// <summary>
		/// Dim the light from 100 to 0 (for LB100, only increments of 10 will work)
		/// </summary>
		public int brightness { get; set; } = 100;
	}

	public class bulb
	{
		/// <summary>
		/// This variable contains the remote address of the bulb to be controlled. Although you must specify a value for this 
		/// variable at instance creation, you can programmatically change it at any time.
		/// </summary>
		public string bulbAddress = string.Empty;

		/// <summary>
		/// This variable contains the remote port of the bulb to be controlled. Although you must specify a value for this 
		/// variable at instance creation - or leave it as default, you can programmatically change it at any time.
		/// </summary>
		public int bulbPort = 0;

		/// <summary>
		/// Class used to control specific options on TP-Link Kasa Smart Bulbs.
		/// </summary>
		/// <param name="hostAddress">Remote address of the bulb to control</param>
		/// <param name="hostPort">Remote address of the bulb to control - default is 9999</param>
		public bulb(string hostAddress, int hostPort = 9999)
		{
			bulbAddress = hostAddress;
			bulbPort = hostPort;
		}

		#region Helping classes and structures

		/// <summary>
		/// Contains a list of acceptable percentages the bulb understands.
		/// </summary>
		public enum Percentage : int
		{
			_5 = 5,
			_10 = 10,
			_15 = 15,
			_20 = 20,
			_25 = 25,
			_30 = 30,
			_35 = 35,
			_40 = 40,
			_45 = 45,
			_50 = 50,
			_55 = 55,
			_60 = 60,
			_65 = 65,
			_70 = 70,
			_75 = 75,
			_80 = 80,
			_85 = 85,
			_90 = 90,
			_95 = 95,
			_100 = 100,
		}

		/// <summary>
		/// Contains a list of acceptable transition periods the bulb understands.
		/// </summary>
		public enum TransitionPeriod : int
		{
			No_Transition = 1,
			Quarter_of_a_second = 250,
			Half_of_a_second = 500,
			Three_quarters_of_a_second = 750,
			One_Second = 1000,
			One_Second_and_one_quarter = 1250,
			One_Second_and_a_half = 1500,
			One_Second_and_three_quarters = 1750,
			Two_Seconds = 2000,
			Two_Second_and_one_quarter = 2250,
			Two_Second_and_a_half = 2500,
			Two_Second_and_three_quarters = 2750,
			Three_Seconds = 3000,
		}

		#endregion

		/// <summary>
		/// Requests that the bulb changes the colour & brightness to new values with a specific transition time. Since this method is async, you can call it 
		/// multiple times without having to wait for the bulb to respond. This does mean, however, you can't get the response from the bulb.
		/// </summary>
		/// <param name="colour">The colour the bulb should change to</param>
		/// <param name="brightness">The brightness percentage the bulb should change to</param>
		/// <param name="tPeriod">The time between the request and the finished result</param>
		public void SetColourAsync(Color colour, Percentage brightness = Percentage._100, TransitionPeriod tPeriod = TransitionPeriod.Quarter_of_a_second)
		{
			var state = new BulbState();
			state.transition_light_state.on_off = 1;
			state.transition_light_state.color_temp = 0;
			state.transition_light_state.hue = (int)Math.Round(colour.GetHue(), 1);
			state.transition_light_state.saturation = Convert.ToInt32(
				((int)Math.Round(colour.GetSaturation() * 100))
				.ToString()
				);
			state.transition_light_state.brightness = (int)brightness;
			state.transition_light_state.transition_period = (int)tPeriod;

			network.send_BulbMessageInNewThread(bulbAddress, state, bulbPort);
		}

		/// <summary>
		/// Requests that the bulb changes the colour & brightness to new values with a specific transition time. Since this method isn't async, you'll have to wait
		/// for the bulb to respond before you can call it again - this means, however, you can get the response from the bulb.
		/// </summary>
		/// <param name="colour">The colour the bulb should change to</param>
		/// <param name="brightness">The brightness percentage the bulb should change to</param>
		/// <param name="tPeriod">The time between the request and the finished result</param>
		/// <returns>The response from the bulb</returns>
		public dynamic SetColour(Color colour, Percentage brightness = Percentage._100, TransitionPeriod tPeriod = TransitionPeriod.Quarter_of_a_second)
		{
			var state = new BulbState();
			state.transition_light_state.on_off = 1;
			state.transition_light_state.color_temp = 0;
			state.transition_light_state.hue = (int)Math.Round(colour.GetHue(), 1);
			state.transition_light_state.saturation = Convert.ToInt32(
				((int)Math.Round(colour.GetSaturation() * 100))
				.ToString()
				);
			state.transition_light_state.brightness = (int)brightness;
			state.transition_light_state.transition_period = (int)tPeriod;

			return network.send_BulbMessage(bulbAddress, state, bulbPort);
		}

		/// <summary>
		/// Requests that the bulb turn itself off. This method waits for a resposne before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic TurnOff()
		{
			var state = new BulbState();
			state.transition_light_state.on_off = 0;

			return network.send_BulbMessage(bulbAddress, state, bulbPort);
		}

		/// <summary>
		/// Requests that the bulb turn itself on. The bulb should resume its last known state. This method waits for a resposne before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic TurnOn()
		{
			var state = new BulbState();
			state.transition_light_state.on_off = 1;

			return network.send_BulbMessage(bulbAddress, state, bulbPort);
		}

		/// <summary>
		/// Requests that the bulb send back information about itself. This methods waits for a response before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic Status()
		{
			return network.get_BulbStatus(bulbAddress, bulbPort);
		}

		/// <summary>
		/// Converts a dynamic response object to a string equivalent. Returns an indented string version of the input passed.
		/// </summary>
		/// <param name="data">dynamic response object</param>
		/// <returns>string equivalent</returns>
		public string ToString(dynamic data)
        {
			return JsonConvert.SerializeObject(data, Formatting.Indented);
		}
	}

	public class plug
	{
		/// <summary>
		/// This variable contains the remote address of the plug to be controlled. Although you must specify a value for this 
		/// variable at instance creation, you can programmatically change it at any time.
		/// </summary>
		public string plugAddress = string.Empty;

		/// <summary>
		/// This variable contains the remote port of the plug to be controlled. Although you must specify a value for this 
		/// variable at instance creation - or leave it as default, you can programmatically change it at any time.
		/// </summary>
		public int plugPort = 0;

		/// <summary>
		/// Class used to control specific options on TP-Link Kasa Smart Plugs.
		/// </summary>
		/// <param name="hostAddress">Remote address of the plug to control</param>
		/// <param name="hostPort">Remote address of the plug to control - default is 9999</param>
		public plug(string hostAddress, int hostPort = 9999)
		{
			plugAddress = hostAddress;
			plugPort = hostPort;
		}

		/// <summary>
		/// Requests that the smart plug turn its relay state to 'on'. This method returns 
		/// the response from the smart plug and awaits response before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic TurnOn()
		{
			return network.SendToSmartPlugOrSwitch(plugAddress, Commands.TurnOn(), plugPort);
		}

		/// <summary>
		/// Requests that the smart plug turn its relay state to 'off'. This method returns 
		/// the response from the smart plug and awaits response before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic TurnOff()
		{
			return network.SendToSmartPlugOrSwitch(plugAddress, Commands.TurnOff(), plugPort);
		}

		/// <summary>
		/// Requests that the smart plug send back information about itself and the emeter if 
		/// a compatible model is being used. This method awaits a response before returning.
		/// </summary>
		/// <returns></returns>
		public dynamic Information()
		{
			return network.SendToSmartPlugOrSwitch(plugAddress, Commands.SysInfoAndEmeter(), plugPort);
		}
		
		/// <summary>
		/// Contains a list of acceptable current power states
		/// </summary>
		public enum CurrentState : int
		{
			Off = 0,
			On = 1,
		}

		/// <summary>
		/// Gets the current relay state of the plug. This can either be 'off' or 'on'.
		/// </summary>
		/// <returns></returns>
		public CurrentState State()
                {
			dynamic plugResponse = Information();
			string state = JsonConvert.SerializeObject(plugResponse, Formatting.Indented);

			if (state.ToLower().Contains(@"""relay_state"": 0,"))
			{
				return CurrentState.Off;
			}
			else
			{
				return CurrentState.On;
			}
		}

		/// <summary>
		/// Converts a dynamic response object to a string equivalent. Returns an indented string version of the input passed.
		/// </summary>
		/// <param name="data">dynamic response object</param>
		/// <returns>string equivalent</returns>
		public string ToString(dynamic data)
		{
			return JsonConvert.SerializeObject(data, Formatting.Indented);
		}
	}

	private static class Commands
	{
		public static string TurnOff()
		{
			return JsonConvert.SerializeObject(new
			{
				system = new
				{
					set_relay_state = new
					{
						state = 0
					}
				}
			});
		}
		public static string TurnOn()
		{
			return JsonConvert.SerializeObject(new
			{
				system = new
				{
					set_relay_state = new
					{
						state = 1
					}
				}
			});
		}
		public static string SysInfo()
		{
			return JsonConvert.SerializeObject(new
			{
				system = new
				{
					get_sysinfo = new
					{
					}
				}
			});
		}
		public static string SysInfoAndEmeter()
		{
			return JsonConvert.SerializeObject(new
			{
				system = new
				{
					get_sysinfo = new
					{
					}
				},
				emeter = new
				{
					get_realtime = new { },
					get_vgain_igain = new { }
				}
			});
		}
		public static string Emeter()
		{
			return JsonConvert.SerializeObject(new
			{
				emeter = new
				{
					get_realtime = new { },
					get_vgain_igain = new { }
				}
			});
		}
		public static string MonthStats(int year)
		{
			return JsonConvert.SerializeObject(new
			{
				emeter = new
				{
					get_monthstat = new { year = year },
				}
			});
		}
	}

	private static class network
	{
		public static dynamic SendToSmartDevice(string ip, string data, SocketType socketType, ProtocolType protocolType, int port)
		{
			try
			{
				using (var sender = new Socket(AddressFamily.InterNetwork, socketType, protocolType))
				{
					var tpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
					sender.Connect(tpEndPoint);
					sender.Send(Encrypt(data, protocolType == ProtocolType.Tcp));
					byte[] buffer = new byte[2048];
					sender.ReceiveTimeout = 5000;
					EndPoint recEndPoint = tpEndPoint;

					int bytesLen = sender.Receive(buffer);
					if (bytesLen > 0)
					{
						return JsonConvert.DeserializeObject<dynamic>(Decrypt(buffer.Take(bytesLen).ToArray(), protocolType == ProtocolType.Tcp));
					}
					else
					{
						throw new Exception("No answer...something went wrong");
					}
				}
			}
			catch { return ""; }
		}

		public static void send_BulbMessageInNewThread(string ip, BulbState transition_light_state, int port = 9999)
		{
			new Thread(() =>
			{
				Thread.CurrentThread.IsBackground = true;

				string data = $"{{\"smartlife.iot.smartbulb.lightingservice\":{JsonConvert.SerializeObject(transition_light_state)}}}";
				SendToSmartDevice(ip, data, SocketType.Dgram, ProtocolType.Udp, port);
			}).Start();
		}

		public static dynamic send_BulbMessage(string ip, BulbState transition_light_state, int port = 9999)
		{
			string data = $"{{\"smartlife.iot.smartbulb.lightingservice\":{JsonConvert.SerializeObject(transition_light_state)}}}";
			return SendToSmartDevice(ip, data, SocketType.Dgram, ProtocolType.Udp, port);
		}

		public static dynamic get_BulbStatus(string ip, int port = 9999)
		{
			string data = "{\"smartlife.iot.smartbulb.lightingservice\": {\"get_light_details\": {}}}";
			return SendToSmartDevice(ip, data, SocketType.Dgram, ProtocolType.Udp, port);
		}

		public static dynamic SendToSmartPlugOrSwitch(string ip, string data, int port = 9999)
		{
			return SendToSmartDevice(ip, data, SocketType.Stream, ProtocolType.Tcp, port);
		}

		private static UInt32 ReverseBytes(UInt32 value)
		{
			return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
				   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
		}
		private static byte[] Encrypt(string payload, bool hasHeader = true)
		{
			byte key = 0xAB;
			byte[] cipherBytes = new byte[payload.Length];
			byte[] header = hasHeader ? BitConverter.GetBytes(ReverseBytes((UInt32)payload.Length)) : new byte[] { };
			for (var i = 0; i < payload.Length; i++)
			{
				cipherBytes[i] = Convert.ToByte(payload[i] ^ key);
				key = cipherBytes[i];
			}
			return header.Concat(cipherBytes).ToArray();
		}
		private static string Decrypt(byte[] cipher, bool hasHeader = true)
		{
			byte key = 0xAB;
			byte nextKey;
			if (hasHeader)
				cipher = cipher.Skip(4).ToArray();
			byte[] result = new byte[cipher.Length];

			for (int i = 0; i < cipher.Length; i++)
			{
				nextKey = cipher[i];
				result[i] = (byte)(cipher[i] ^ key);
				key = nextKey;
			}
			return Encoding.UTF7.GetString(result);
		}
	}

}
