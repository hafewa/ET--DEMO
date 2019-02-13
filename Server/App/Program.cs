using System;
using System.Threading;
using ETModel;
using NLog;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
			
			try
			{			
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());

				Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = $"{startConfig.AppType}";
				LogManager.Configuration.Variables["appId"] = $"{startConfig.AppId}";
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType, -8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:0000}";

				Log.Info($"server start........................ {startConfig.AppId} {startConfig.AppType}");

				Game.Scene.AddComponent<TimerComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatcherComponent>();

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				
				switch (startConfig.AppType)
				{
                        //管理服务器
                        //连接客户端的外网和连接内部服务器的内网，对服务器进程进行管理，自动检测和启动服务器进程
					case AppType.Manager:
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						break;

                        //登录服务器，客户端连接的第一个服务器
                        //对ActorMessage消息进行管理（添加，移除，分发等）
                        //连接内网和外网，对内网服务器进程进行操作
                        //随机分发Gate服务器地址
					case AppType.Realm:
						Game.Scene.AddComponent<MailboxDispatcherComponent>();
						Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						break;

                        //对玩家进行管理
                        //对ActorMessage消息进行管理（添加，移除，分发）
                        //连接内网和外网，对内网服务器进程进行操作
                        //随机分配Gate服务器地址
                        //对Actor消息进程进行管理
                        //对玩家ID登陆后的Key进行管理
					case AppType.Gate:
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<MailboxDispatcherComponent>();
						Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;

                        //连接内网
                        //服务器进程状态集中管理（Actor消息IP管理服务器）
                        //对客户端的登陆消息进行验证和客户端登陆后连接的服务器
                        //登陆后通过此服务器进行消息互动，也可称为验证服务器
					case AppType.Location:
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<LocationComponent>();
						break;

                        //连接内网
                        //对ActorMessage消息进行管理（添加，移除，分发等）
                        //对场景内现在的活动物体存储管理
                        //对内网服务器进程进行操作
                        //对Actor消息进程进行管理
                        //服务器帧率管理                   
					case AppType.Map:
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						Game.Scene.AddComponent<MailboxDispatcherComponent>();
						Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
						Game.Scene.AddComponent<PathfindingComponent>();
						break;

                        //以上服务器功能集中合并成一个服务器  另外增加DB连接组件  DB管理组件
					case AppType.AllServer:
						// 发送普通actor消息
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						
						// 发送location actor消息
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						
						//Game.Scene.AddComponent<DBComponent>();
						//Game.Scene.AddComponent<DBProxyComponent>();
						
						// location server需要的组件
						Game.Scene.AddComponent<LocationComponent>();
						
						// 访问location server的组件
						Game.Scene.AddComponent<LocationProxyComponent>();
						
						// 这两个组件是处理actor消息使用的
						Game.Scene.AddComponent<MailboxDispatcherComponent>();
						Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
						
						// 内网消息组件
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						
						// 外网消息组件
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						
						// manager server组件，用来管理其它进程使用
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						
						// 配置管理
						Game.Scene.AddComponent<ConfigComponent>();
						
						// recast寻路组件
						Game.Scene.AddComponent<PathfindingComponent>();
						
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<UnitComponent>();

						Game.Scene.AddComponent<ConsoleComponent>();
						// Game.Scene.AddComponent<HttpComponent>();
						break;

                        //连接内网和测试服务器承受力
					case AppType.Benchmark:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmarkComponent, string>(clientConfig.Address);
						break;
					case AppType.BenchmarkWebsocketServer:
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						break;
					case AppType.BenchmarkWebsocketClient:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<WebSocketBenchmarkComponent, string>(clientConfig.Address);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}
				
				while (true)
				{
					try
					{
						Thread.Sleep(1);
						OneThreadSynchronizationContext.Instance.Update();
						Game.EventSystem.Update();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
