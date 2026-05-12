using Microsoft.AspNetCore.Mvc;
using ClickAndCollect.Models;

namespace ClickAndCollect.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            List<Client> clients = new List<Client>();
            Client c1 = new Client(1, "Marc", "Demarco", "client1@gmail.com", "1234");
            clients.Add(c1);
            Client c2 = new Client(2, "Jean", "Delafontaine", "client2@gmail.com", "1234");
            clients.Add(c2);

            List<Order> orders = new List<Order>();
            orders.Add(new Order(1, c2));
            orders.Add(new Order(2, c2));
            orders.Add(new Order(3, c2));
            orders.Add(new Order(4, c1));

            return View(orders);
        }
    }
}
