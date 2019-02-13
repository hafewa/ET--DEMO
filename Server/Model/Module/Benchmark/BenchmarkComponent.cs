namespace ETModel
{
    /// <summary>
    /// 服务器承受力测试组件
    /// </summary>
	public class BenchmarkComponent: Component
	{
		public int k;

		public long time1 = TimeHelper.ClientNow();
	}
}