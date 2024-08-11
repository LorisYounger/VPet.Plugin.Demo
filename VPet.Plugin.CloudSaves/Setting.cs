using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.CloudSaves
{
    public class Setting
    {
        /// <summary>
        /// 服务器链接
        /// </summary>
        [Line]
        public string ServerURL { get; set; }
        /// <summary>
        /// 链接秘钥(密码)
        /// </summary>
        [Line]
        public ulong Passkey { get; set; }

        /// <summary>
        /// 定时备份时间(分钟)
        /// </summary>
        [Line]
        public int BackupTime { get => backupTime; set => backupTime = Math.Min(24 * 60, Math.Max(5, value)); }

        private int backupTime = 20;


    }
}
