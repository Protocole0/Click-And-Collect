using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ClickAndCollect.Models
{
    public class Order
    {
        public const decimal ServiceFee = 5.95m;

        private List<OrderLine> _lines;

        public List<OrderLine> Lines
        {
            get => _lines;
            set { _lines = value; }
        }

        public int TotalItems()
        {
            int total = 0;
            foreach (OrderLine line in _lines)
                total += line.Quantity;
            return total;
        }

        public decimal TotalAmount()
        {
            decimal total = 0;
            foreach (OrderLine line in _lines)
                total += line.Quantity * line.Product.Price;
            return total;
        }

        public Order()
        {
            _lines = new List<OrderLine>();
        }

        // --- Méthode d'instance : l'objet Order communique avec la classe Product ---

        public void AddProduct(Product product, int quantity)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == product.ProductId)
                {
                    line.Quantity += quantity;
                    return;
                }
            }
            _lines.Add(new OrderLine(product, quantity));
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == productId)
                {
                    line.Quantity = quantity;
                    return;
                }
            }
        }

        public void RemoveLine(int productId)
        {
            foreach (OrderLine line in _lines)
            {
                if (line.Product.ProductId == productId)
                {
                    _lines.Remove(line);
                    return;
                }
            }
        }

        // --- Méthodes statiques : gestion de la session ---

        public static Order GetFromSession(ISession session)
        {
            string? json = session.GetString("cart");
            if (json == null)
                return new Order();
            return JsonSerializer.Deserialize<Order>(json) ?? new Order();
        }

        public void SaveToSession(ISession session)
        {
            session.SetString("cart", JsonSerializer.Serialize(this));
        }
    }
}
