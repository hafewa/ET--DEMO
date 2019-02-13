
namespace ETHotfix
{
    /// <summary>
    /// 窗体层级
    /// </summary>
    public static class WindowLayer
    {
        public const string UIHiden = "UIHiden";

        //表示横屏
        public const string LBottom = "LBottom";   //底层  一般用来放置最底层的UI
        public const string LMedium = "LMedium";   //中间层  比较常用，大部分界面均放在此层
        public const string LTop = "LTop";         //上层  一般用来放各种弹窗，小窗口
        public const string LTopMost = "LTopMost";  //最上层 一般用来做各种遮罩层 屏蔽输入或者切入动画

        //表示竖屏
        public const string PBottom = "PBottom";
        public const string PMedium = "PMedium";
        public const string PTop = "PTop";
        public const string PTopMost = "PTopMost";
    }
}
