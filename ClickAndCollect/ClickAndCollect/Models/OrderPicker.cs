namespace ClickAndCollect.Models
{
    public class OrderPicker : User
    {
        private Store? _store;
        public Store? Store { get => _store; set => _store = value; }

        public OrderPicker(int id, string email, Store store) : base(id, email, string.Empty)
        {
            _store = store;
        }
    }
}
