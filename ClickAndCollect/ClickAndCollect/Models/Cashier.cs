namespace ClickAndCollect.Models
{
    public class Cashier : User
    {
        private Store? _store;
        public Store? Store { get => _store; set => _store = value; }

        public Cashier(int id, string email, Store store) : base(id, email, string.Empty)
        {
            _store = store;
        }
    }
}
