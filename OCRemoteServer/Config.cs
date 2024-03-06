namespace OCRemoteServer
{
    public static class Config
    {
        // 存放能源站监控数据库的位置
        public static readonly string DatabasePath = "../es.db";
        
        // 通过 NEI 导出的物品面板文件，本项目已经自带 2.5.1 的内容 (～￣(OO)￣)ブ
        public static readonly string ItemPanelPath = "../itempanel.csv";

        // 通过 NEI 导出的物品面板文件夹，文件夹名应该为 itempanel_icons
        public static readonly string ItemPanelIconFolderPath = @"C:\Users\cyl18\Documents\Persistance\MultiMC\instances\GT_New_Horizons_2.4.1_Java_17-20\.minecraft\dumps\itempanel_icons\";

        // blazorise 令牌内容，你可以找 Cyl18 要，没有这个的话 Web 界面会显示异常
        public static readonly string BlazoriseToken = File.ReadAllText("../blazorise.txt").Trim();


    }
}
