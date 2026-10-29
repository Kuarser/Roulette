using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine.Analytics;
using Newtonsoft.Json;
//各平台下数据库存储的绝对路径(通用)
//PC：sql = new SQLiteHelper("data source=" + Application.dataPath + "/Fire_Line.db");
//Mac：sql = new SQLiteHelper("data source=" + Application.dataPath + "/Fire_Line.db");
//Android：sql = new SQLiteHelper("URI=file:" + Application.persistentDataPath + "/Fire_Line.db");
//iOS：sql = new SQLiteHelper("data source=" + Application.persistentDataPath + "/Fire_Line.db");

//PC平台下的相对路径
//sql = new SQLiteHelper("data source="sqlite4unity.db");
//编辑器：Assets/sqlite4unity.db
//编译后：和AppName.exe同级的文件夹下，这里比較奇葩
//当然能够用更任意的方式sql = new SQLiteHelper("data source="D://SQLite//Fire_Line.db");
//确保路径存在就可以否则会错误发生

//假设是事先创建了一份数据库
//能够将这个数据库放置在StreamingAssets文件夹下然后再复制到
//Application.persistentDataPath + "/Fire_Line.db"路径就可以

public class SQLite : BaseManager<SQLite>
{
    SQLiteHelper sql;
    string dbPath;

    public void Init()
    {
#if UNITY_EDITOR//在unity编辑模式下
        dbPath = Application.dataPath + "/game_db.db";
#elif UNITY_ANDROID//ANDROID平台
        dbPath = Application.persistentDataPath + "/game_db.db";
#endif
        InitSqlConnection();
        CreateDefaultTable();
    }

    private void CreateDefaultTable()
    {
        CreatPlayerRecordTable();
    }

    void InitSqlConnection()
    {
        //创建数据库连接
#if UNITY_EDITOR//在unity编辑模式下
        sql = new SQLiteHelper("data source=" + dbPath);
#elif UNITY_ANDROID//ANDROID平台
        sql = new SQLiteHelper("URI=file:" + dbPath);
#endif
    }
    //-------------------------------------------------------------------------------------------------游戏数据表
    private void CreatPlayerRecordTable()
    {
        sql.CreateTable("player_record", new string[] { "player_id", "data" },
            new string[] { "INTEGER", "TEXT" });
        SqliteDataReader reader = sql.ExecuteQuery("SELECT * FROM player_record");
        if (!reader.HasRows)
        {
            for (int i = 1; i < 11; i++)
            {
                string[] saveValue = new string[] { i.ToString(), "'[]'" };
                InsertValues("player_record", saveValue);
            }
        }
    }

    public void UpdateRecordData2(int playerid, string data)
    {
        // 根据 playerid 更新 player_record 表中的 data 字段
        string[] saveName = new string[] { "data" };
        string sanitizedData = data.Replace("'", "''");
        string[] saveValue = new string[] { "'" + sanitizedData + "'" };
        string[] whereName = new string[] { "player_id" };
        string[] operations = new string[] { "=" };
        string[] whereValue = new string[] { playerid.ToString() };
        UpdataData("player_record", saveName, saveValue, whereName, operations, whereValue);
    }

    public string ReadRecordData2(int playerID)
    {
        // 读取 player_record 表中对应玩家ID的数据
        SqliteDataReader reader = sql.ExecuteQuery("SELECT * FROM player_record WHERE player_id = " + playerID + " order by player_id ASC");
        string records = "";
        while (reader.Read())
        {
            records = reader.GetString(reader.GetOrdinal("data"));
        }
        reader.Close();
        return records;
    }

    public void InsertValues(string tableName, string[] saveValue)
    {
        sql.InsertValues(tableName, saveValue);
    }

    public void UpdataData(string tableName, string[] saveName, string[] saveValue, string[] whereName, string[] operations, string[] whereValue)
    {
        sql.UpdateTable(tableName, saveName, saveValue, whereName, operations, whereValue);
    }

    public void CloseConnection()
    {
        sql.CloseConnection();
    }

    public void ClearReacord()
    {
        sql.ClearTable("player_record");
        CreateDefaultTable();               //清除之后需要重建，不然player_record无法直接更新
    }
}


