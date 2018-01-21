﻿using Binance.Net.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Binance.Net.Objects
{
    /// <summary>
    /// Wrapper for list of deposits
    /// </summary>
    public class BinanceDepositList
    {
        /// <summary>
        /// The list of deposits
        /// </summary>
        [JsonProperty("depositList")]
        public List<BinanceDeposit> List { get; set; }
        /// <summary>
        /// Boolean indicating if the deposit list retrieval was successful
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Message what went wrong if retrieving wasn't successful
        /// </summary>
        [JsonProperty("msg")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Information about a deposit
    /// </summary>
    public class BinanceDeposit
    {
        /// <summary>
        /// Time the deposit was added to Binance
        /// </summary>
        [JsonConverter(typeof(TimestampConverter))]
        public DateTime InsertTime { get; set; }
        /// <summary>
        /// The amount deposited
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// The asset deposited
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// The status of the deposit
        /// </summary>
        [JsonConverter(typeof(DepositStatusConverter))]
        public DepositStatus Status { get; set; }
    }
}
