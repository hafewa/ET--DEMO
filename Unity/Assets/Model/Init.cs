using System;
using System.Threading;
using UnityEngine;

namespace ETModel
{
	public class Init : MonoBehaviour
	{
		private void Start()
		{
			this.StartAsync().Coroutine();
		}
		
		private async ETVoid StartAsync()
		{
			try
			{
                //给Socket线程队列同步用的
				SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

				DontDestroyOnLoad(gameObject);
                
                //反射当前所有的dll
				Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);
                
              
				Game.Scene.AddComponent<TimerComponent>();

                //全局配置文件（服务器的链接IP  资源服务器的HTTP地址）
                Game.Scene.AddComponent<GlobalConfigComponent>();
                //网络组件，进行服务器通讯的额组件
				Game.Scene.AddComponent<NetOuterComponent>();
                //资源管理组件（AB包）
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<PlayerComponent>();
                //单元组件（熊猫用来做帧同步  Demo用）
				Game.Scene.AddComponent<UnitComponent>();
                //ET的UI框架
				Game.Scene.AddComponent<UIComponent>();

				// 下载ab包
				await BundleHelper.DownloadBundle();

                //读取热更代码（调用ILRuntime）
				Game.Hotfix.LoadHotfixAssembly();

				// 加载配置
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");

                //消息识别码组件
				Game.Scene.AddComponent<OpcodeTypeComponent>();

                //消息分发组件
				Game.Scene.AddComponent<MessageDispatcherComponent>();

				Game.Hotfix.GotoHotfix();

                //测试代码   可以删
				Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void Update()
		{
			OneThreadSynchronizationContext.Instance.Update();
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}