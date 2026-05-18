using ClickAndCollect.Interfaces;

namespace ClickAndCollect.Models
{
    public class Store
    {
        private int _storeId;
        public int StoreId { get => _storeId; set => _storeId = value; }

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _streetName;
        public string StreetName { get => _streetName; set => _streetName = value; }

        private string _streetNumber;
        public string StreetNumber { get => _streetNumber; set => _streetNumber = value; }

        private string _city;
        public string City { get => _city; set => _city = value; }

        private string _postalCode;
        public string PostalCode { get => _postalCode; set => _postalCode = value; }

        public string FullAddress => $"{_streetName} {_streetNumber}, {_postalCode} {_city}";

        public Store() { _name = _streetName = _streetNumber = _city = _postalCode = string.Empty; }

        public Store(int storeId, string name, string streetName, string streetNumber, string city, string postalCode)
        {
            _storeId      = storeId;
            _name         = name;
            _streetName   = streetName;
            _streetNumber = streetNumber;
            _city         = city;
            _postalCode   = postalCode;
        }

        // --- Méthodes statiques : la classe délègue au DAL ---

        public static async Task<List<Store>> GetAll(IStoreDAL storeDAL)
        {
            return await storeDAL.GetAllAsync();
        }

        public static async Task<Store?> GetById(int storeId, IStoreDAL storeDAL)
        {
            return await storeDAL.GetByIdAsync(storeId);
        }
    }
}
