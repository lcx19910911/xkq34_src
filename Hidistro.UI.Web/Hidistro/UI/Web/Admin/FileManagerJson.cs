namespace Hidistro.UI.Web.Admin
{
    using Hidistro.Core;
    using Hidistro.UI.ControlPanel.Utility;
    using LitJson;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class FileManagerJson : AdminPage
    {
        private string dir;

        protected FileManagerJson() : base("m01", "00000")
        {
            this.dir = "image";
        }

        public void FillTableForDb(string cid, string order, Hashtable table)
        {
            int result = 0;
            int.TryParse(cid, out result);
            Database database = DatabaseFactory.CreateDatabase();
            table["moveup_dir_path"] = "";
            table["current_dir_path"] = "";
            table["current_url"] = "";
            new List<Hashtable>();
            List<Hashtable> list = new List<Hashtable>();
            table["file_list"] = list;
            if (result > 0)
            {
                Hashtable item = new Hashtable();
                item["is_dir"] = true;
                item["has_file"] = true;
                item["filesize"] = 0;
                item["is_photo"] = false;
                item["filetype"] = "";
                item["filename"] = "上级目录";
                item["path"] = "-1";
                item["datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                list.Add(item);
            }
            if (result <= 0)
            {
                string str = "select CategoryId,CategoryName from Hishop_PhotoCategories order by DisplaySequence";
                DbCommand command = database.GetSqlStringCommand(str);
                using (IDataReader reader = database.ExecuteReader(command))
                {
                    while (reader.Read())
                    {
                        int categoryFile = 0;
                        decimal fileSize = 0M;
                        categoryFile = this.GetCategoryFile((int) reader["CategoryId"], out fileSize);
                        Hashtable hashtable2 = new Hashtable();
                        hashtable2["is_dir"] = true;
                        hashtable2["has_file"] = categoryFile > 0;
                        hashtable2["filesize"] = fileSize;
                        hashtable2["is_photo"] = false;
                        hashtable2["filetype"] = "";
                        hashtable2["filename"] = reader["CategoryName"];
                        hashtable2["cid"] = reader["CategoryId"];
                        hashtable2["path"] = reader["CategoryName"];
                        hashtable2["datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        list.Add(hashtable2);
                    }
                }
            }
            string query = string.Format("select * from Hishop_PhotoGallery where CategoryId=0 order by {0}", order);
            if (result > 0)
            {
                query = string.Format("select * from Hishop_PhotoGallery where CategoryId={0} order by {1}", result, order);
            }
            DbCommand sqlStringCommand = database.GetSqlStringCommand(query);
            using (IDataReader reader2 = database.ExecuteReader(sqlStringCommand))
            {
                while (reader2.Read())
                {
                    Hashtable hashtable3 = new Hashtable();
                    hashtable3["cid"] = reader2["CategoryId"];
                    hashtable3["name"] = reader2["PhotoName"];
                    hashtable3["path"] = reader2["PhotoPath"];
                    hashtable3["filename"] = reader2["PhotoName"];
                    hashtable3["filesize"] = reader2["FileSize"];
                    hashtable3["addedtime"] = reader2["UploadTime"];
                    hashtable3["updatetime"] = reader2["LastUpdateTime"];
                    hashtable3["datetime"] = (reader2["LastUpdateTime"] == DBNull.Value) ? "" : ((DateTime) reader2["LastUpdateTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                    string str2 = reader2["PhotoPath"].ToString().Trim();
                    hashtable3["filetype"] = str2.Substring(str2.LastIndexOf('.'));
                    list.Add(hashtable3);
                }
            }
            table["total_count"] = list.Count;
            table["current_cateogry"] = result;
        }

        public void FillTableForPath(string dirPath, string url, string order, Hashtable table)
        {
            string path = "";
            string str2 = "";
            string input = "";
            string str4 = "";
            string str5 = base.Request.QueryString["path"];
            str5 = string.IsNullOrEmpty(str5) ? "" : str5;
            if (Regex.IsMatch(str5, @"\.\."))
            {
                base.Response.Write("Access is not allowed.");
                base.Response.End();
            }
            if ((str5 != "") && !str5.EndsWith("/"))
            {
                str5 = str5 + "/";
            }
            if (str5 == "")
            {
                path = dirPath;
                str2 = url;
                input = "";
                str4 = "";
            }
            else
            {
                path = dirPath + str5;
                str2 = url + str5;
                input = str5;
                str4 = Regex.Replace(input, @"(.*?)[^\/]+\/$", "$1");
            }
            path = base.Server.MapPath(path);
            if (!Directory.Exists(path))
            {
                base.Response.Write("此目录不存在！" + path);
                base.Response.End();
            }
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            switch (order)
            {
                case "uploadtime":
                    Array.Sort(files, new DateTimeSorter(0, true));
                    break;

                case "uploadtime desc":
                    Array.Sort(files, new DateTimeSorter(0, false));
                    break;

                case "lastupdatetime":
                    Array.Sort(files, new DateTimeSorter(1, true));
                    break;

                case "lastupdatetime desc":
                    Array.Sort(files, new DateTimeSorter(1, false));
                    break;

                case "photoname":
                    Array.Sort(files, new NameSorter(true));
                    break;

                case "photoname desc":
                    Array.Sort(files, new NameSorter(false));
                    break;

                case "filesize":
                    Array.Sort(files, new SizeSorter(true));
                    break;

                case "filesize desc":
                    Array.Sort(files, new SizeSorter(false));
                    break;

                default:
                    Array.Sort(files, new NameSorter(true));
                    break;
            }
            table["moveup_dir_path"] = str4;
            table["current_dir_path"] = input;
            table["current_url"] = str2;
            table["total_count"] = directories.Length + files.Length;
            List<Hashtable> list = new List<Hashtable>();
            table["file_list"] = list;
            if (str5 != "")
            {
                Hashtable item = new Hashtable();
                item["is_dir"] = true;
                item["has_file"] = true;
                item["filesize"] = 0;
                item["is_photo"] = false;
                item["filetype"] = "";
                item["filename"] = "";
                item["path"] = "";
                item["datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                item["filename"] = "上级目录";
                list.Add(item);
            }
            for (int i = 0; i < directories.Length; i++)
            {
                DirectoryInfo info = new DirectoryInfo(directories[i]);
                Hashtable hashtable2 = new Hashtable();
                hashtable2["is_dir"] = true;
                hashtable2["has_file"] = info.GetFileSystemInfos().Length > 0;
                hashtable2["filesize"] = 0;
                hashtable2["is_photo"] = false;
                hashtable2["filetype"] = "";
                hashtable2["filename"] = info.Name;
                hashtable2["path"] = info.Name;
                hashtable2["datetime"] = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                list.Add(hashtable2);
            }
            for (int j = 0; j < files.Length; j++)
            {
                FileInfo info2 = new FileInfo(files[j]);
                Hashtable hashtable3 = new Hashtable();
                hashtable3["cid"] = "-1";
                hashtable3["name"] = info2.Name;
                hashtable3["path"] = url + str5 + info2.Name;
                hashtable3["filename"] = info2.Name;
                hashtable3["filesize"] = info2.Length;
                hashtable3["addedtime"] = info2.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                hashtable3["updatetime"] = info2.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                hashtable3["datetime"] = info2.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                hashtable3["filetype"] = info2.Extension.Substring(1);
                list.Add(hashtable3);
            }
        }

        public int GetCategoryFile(int iCategryId, out decimal fileSize)
        {
            int num = 0;
            fileSize = 0M;
            Database database = DatabaseFactory.CreateDatabase();
            string query = string.Format("select Count(PhotoId) as FileCount,isnull(Sum(FileSize),0) as AllFileSize from Hishop_PhotoGallery", new object[0]);
            if (iCategryId > 0)
            {
                query = string.Format("select Count(PhotoId) as FileCount,isnull(Sum(FileSize),0) as AllFileSize from Hishop_PhotoGallery where CategoryId={0} ", iCategryId.ToString());
            }
            DbCommand sqlStringCommand = database.GetSqlStringCommand(query);
            using (IDataReader reader = database.ExecuteReader(sqlStringCommand))
            {
                if (reader.Read())
                {
                    num = (int) reader["FileCount"];
                    fileSize = (int) reader["AllFileSize"];
                }
            }
            return num;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Hashtable table = new Hashtable();
            if (Globals.GetCurrentManagerUserId() == 0)
            {
                base.Response.Write("没有权限！");
                base.Response.End();
            }
            else
            {
                this.dir = base.Request["dir"];
                if (string.IsNullOrEmpty(this.dir))
                {
                    this.dir = "image";
                }
                string dirPath = "";
                string url = "";
                string str3 = "false";
                if (base.Request.QueryString["isAdvPositions"] != null)
                {
                    str3 = base.Request.QueryString["isAdvPositions"].ToString().ToLower().Trim();
                }
                dirPath = "~/Storage/master/gallery/";
                url = "/Storage/master/gallery/";
                dirPath = "~/Storage/master/gallery/";
                url = "/Storage/master/gallery/";
                string dir = this.dir;
                if (dir != null)
                {
                    if (!(dir == "image"))
                    {
                        if (dir == "file")
                        {
                            dirPath = "~/Storage/master/accessory/";
                            url = "/Storage/master/accessory/";
                        }
                        else if (dir == "flash")
                        {
                            dirPath = "~/Storage/master/flash/";
                            url = "/Storage/master/flash/";
                        }
                        else if (dir == "media")
                        {
                            dirPath = "~/Storage/master/media/";
                            url = "/Storage/master/media/";
                        }
                    }
                    else
                    {
                        dirPath = "~/Storage/master/gallery/";
                        url = "/Storage/master/gallery/";
                    }
                }
                string str4 = base.Request.QueryString["order"];
                str4 = string.IsNullOrEmpty(str4) ? "uploadtime" : str4.ToLower();
                string cid = base.Request.QueryString["path"];
                switch (cid)
                {
                    case null:
                    case "":
                        cid = "-1";
                        break;
                }
                if (str3 == "false")
                {
                    this.FillTableForDb(cid, str4, table);
                }
                else
                {
                    this.FillTableForPath(dirPath, url, str4, table);
                }
                string str6 = base.Request.Url.ToString();
                str6 = str6.Substring(0, str6.IndexOf("/", 7)) + base.Request.ApplicationPath;
                if (str6.EndsWith("/"))
                {
                    str6 = str6.Substring(0, str6.Length - 1);
                }
                table["domain"] = str6;
                base.Response.AddHeader("Content-Type", "application/json; charset=UTF-8");
                base.Response.Write(JsonMapper.ToJson(table));
                base.Response.End();
            }
        }

        public class DateTimeSorter : IComparer
        {
            private bool ascend;
            private int type;

            public DateTimeSorter(int sortType, bool isAscend)
            {
                this.ascend = isAscend;
                this.type = sortType;
            }

            public int Compare(object x, object y)
            {
                if ((x == null) && (y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    if (!this.ascend)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (y == null)
                {
                    if (!this.ascend)
                    {
                        return -1;
                    }
                    return 1;
                }
                FileInfo info = new FileInfo(x.ToString());
                FileInfo info2 = new FileInfo(y.ToString());
                if (this.type == 0)
                {
                    if (!this.ascend)
                    {
                        return info2.CreationTime.CompareTo(info.CreationTime);
                    }
                    return info.CreationTime.CompareTo(info2.CreationTime);
                }
                if (!this.ascend)
                {
                    return info2.LastWriteTime.CompareTo(info.LastWriteTime);
                }
                return info.LastWriteTime.CompareTo(info2.LastWriteTime);
            }
        }

        public class NameSorter : IComparer
        {
            private bool ascend;

            public NameSorter(bool isAscend)
            {
                this.ascend = isAscend;
            }

            public int Compare(object x, object y)
            {
                if ((x == null) && (y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    if (!this.ascend)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (y == null)
                {
                    if (!this.ascend)
                    {
                        return -1;
                    }
                    return 1;
                }
                FileInfo info = new FileInfo(x.ToString());
                FileInfo info2 = new FileInfo(y.ToString());
                if (!this.ascend)
                {
                    return info2.FullName.CompareTo(info.FullName);
                }
                return info.FullName.CompareTo(info2.FullName);
            }
        }

        public class SizeSorter : IComparer
        {
            private bool ascend;

            public SizeSorter(bool isAscend)
            {
                this.ascend = isAscend;
            }

            public int Compare(object x, object y)
            {
                if ((x == null) && (y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    if (!this.ascend)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (y == null)
                {
                    if (!this.ascend)
                    {
                        return -1;
                    }
                    return 1;
                }
                FileInfo info = new FileInfo(x.ToString());
                FileInfo info2 = new FileInfo(y.ToString());
                if (!this.ascend)
                {
                    return info2.Length.CompareTo(info.Length);
                }
                return info.Length.CompareTo(info2.Length);
            }
        }
    }
}

