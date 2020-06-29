namespace Product.API.MessageDto
{
    /// <summary>
    /// 下单事件消息
    /// </summary>
    public class CreateOrderMessageDto
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public int Count { get; set; }
    }
}