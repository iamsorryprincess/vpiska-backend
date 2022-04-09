using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using RtclightWeb.Models;

namespace RtclightWeb.Services
{
    public class OfferService
    {
        private readonly IMongoCollection<Offer> _offers;

        public OfferService(IMongoDatabase database)
        {
            _offers = database.GetCollection<Offer>("offers");
        }

        public async Task<Offer> GetLast()
        {
            var cursor = await _offers.FindAsync(Builders<Offer>.Filter.Empty);
            var offers = await cursor.ToListAsync();
            return offers.LastOrDefault();
        }

        public async Task<string> Insert(Offer offer)
        {
            await _offers.InsertOneAsync(offer);
            return offer.Id;
        }
    }
}