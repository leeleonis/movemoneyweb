using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BitcoinTransactionManagement.Models
{
    /// <summary>
    /// 使用者
    /// </summary>
    [MetadataType(typeof(UsersMetadata))]
    public partial class Users
    {
        public class UsersMetadata
        {
            [Required(ErrorMessage = "id未填！")]
            public System.Guid id { get; set; }

            [Required(ErrorMessage = "請輸入姓名！")]
            [MaxLength(25, ErrorMessage = "限輸入25字內")]
            public string Name { get; set; }

            [Required(ErrorMessage = "請輸入帳號！")]
            [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]{5,31}$", ErrorMessage = "帳號需字母開頭，限字母數字與底線，且在6~30個字元內！")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "請填寫密碼！")]
            public string Password { get; set; }
            public string CreateID { get; set; }
            public Nullable<System.DateTime> Createdt { get; set; }
            public string UpdateID { get; set; }
            public Nullable<System.DateTime> Updatedt { get; set; }
            public string DisableID { get; set; }
            public Nullable<System.DateTime> Disabledt { get; set; }
            [Required(ErrorMessage = "Status未填！")]
            public int Status { get; set; }
        }
    }

    /// <summary>
    /// 交易所
    /// </summary>
    [MetadataType(typeof(ExchangeMetadata))]
    public partial class Exchange
    {
        public class ExchangeMetadata
        {
            [Required(ErrorMessage = "id未填！")]
            public System.Guid id { get; set; }

            [Required(ErrorMessage = "請輸入交易所名稱！")]
            [MaxLength(25, ErrorMessage = "限輸入25字內")]
            public string Name { get; set; }

            [Required(ErrorMessage = "請輸入交易所網址！")]
            [MaxLength(100, ErrorMessage = "限輸入100字內")]
            public string ExchangeUrl { get; set; }

            [Required(ErrorMessage = "請輸入手續費率！")]
            public Nullable<decimal> ProcessingFee { get; set; }

            public string CreateID { get; set; }

            public Nullable<System.DateTime> Createdt { get; set; }

            public string UpdateID { get; set; }

            public Nullable<System.DateTime> Updatedt { get; set; }

            public string DisableID { get; set; }

            public Nullable<System.DateTime> Disabledt { get; set; }

            [Required(ErrorMessage = "Status未填！")]
            public int Status { get; set; }
        }
    }

    /// <summary>
    /// 帳戶
    /// </summary>
    [MetadataType(typeof(AccountMetadata))]
    public partial class Account
    {
        public class AccountMetadata
        {
            [Required(ErrorMessage = "id未填！")]
            public System.Guid id { get; set; }

            [Required(ErrorMessage = "ExchangeId未填！")]
            public System.Guid ExchangeId { get; set; }

            [Required(ErrorMessage = "請輸入帳戶名稱！")]
            [MaxLength(25, ErrorMessage = "限輸入25字內")]
            public string Name { get; set; }

            [Required(ErrorMessage = "請輸入金鑰！")]
            [MaxLength(250, ErrorMessage = "限輸入250字內")]
            public string APIKey { get; set; }

            [Required(ErrorMessage = "請輸入Secret！")]
            [MaxLength(250, ErrorMessage = "限輸入250字內")]
            public string Secret { get; set; }

            public string CreateID { get; set; }

            public Nullable<System.DateTime> Createdt { get; set; }

            public string UpdateID { get; set; }

            public Nullable<System.DateTime> Updatedt { get; set; }

            public string DisableID { get; set; }

            public Nullable<System.DateTime> Disabledt { get; set; }

            [Required(ErrorMessage = "Status未填！")]
            public int Status { get; set; }
        }
    }

    /// <summary>
    /// 執行
    /// </summary>
    [MetadataType(typeof(ExecutionsMetadata))]
    public partial class Executions
    {
        public class ExecutionsMetadata
        {
            [Required(ErrorMessage = "id未填！")]
            public System.Guid id { get; set; }

            [Required(ErrorMessage = "請輸入執行名稱！")]
            [MaxLength(25, ErrorMessage = "限輸入25字內")]
            public string Name { get; set; }

            [Required(ErrorMessage = "請輸入最低下單量！")]
            public decimal MinQuantity { get; set; }

            [Required(ErrorMessage = "請輸入最低差價！")]
            public decimal MinDifferencePrices { get; set; }

            public string CreateID { get; set; }

            public Nullable<System.DateTime> Createdt { get; set; }

            public string UpdateID { get; set; }

            public Nullable<System.DateTime> Updatedt { get; set; }

            public string DisableID { get; set; }

            public Nullable<System.DateTime> Disabledt { get; set; }

            [Required(ErrorMessage = "Status未填！")]
            public int Status { get; set; }

            [Required(ErrorMessage = "CurrencyValue未填！")]
            public string CurrencyValue { get; set; }
        }
    }

    /// <summary>
    /// 帳戶餘額
    /// </summary>
    [MetadataType(typeof(FundsBalanceMetadata))]
    public partial class FundsBalance
    {
        public class FundsBalanceMetadata
        {
            [Required(ErrorMessage = "id未填！")]
            public System.Guid AccountId { get; set; }

            [Required(ErrorMessage = "幣別未填！")]
            public string CurrencyVal { get; set; }

            [Required(ErrorMessage = "異動數量未填！")]
            public decimal Quantity { get; set; }

            public string CreateID { get; set; }

            public Nullable<System.DateTime> Createdt { get; set; }
        }
    }

}