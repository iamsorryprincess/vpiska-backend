using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RtclightWeb.Models;

namespace RtclightWeb.Services
{
    public class IceCandidateService
    {
        private readonly IMongoCollection<IceCandidate> _offerCandidates;
        private readonly IMongoCollection<IceCandidate> _answerCandidates;

        public IceCandidateService(IMongoDatabase database)
        {
            _offerCandidates = database.GetCollection<IceCandidate>("ice_candidates_offer");
            _answerCandidates = database.GetCollection<IceCandidate>("ice_candidate_answer");
        }

        public async Task<List<IceCandidate>> GetOfferCandidates()
        {
            var cursor = await _offerCandidates.FindAsync(Builders<IceCandidate>.Filter.Empty);
            var result = await cursor.ToListAsync();
            return result;
        }

        public async Task<List<IceCandidate>> GetAnswerCandidates()
        {
            var cursor = await _answerCandidates.FindAsync(Builders<IceCandidate>.Filter.Empty);
            var result = await cursor.ToListAsync();
            return result;
        }

        public Task InsertOfferCandidate(IceCandidate offerIceCandidate) =>
            _offerCandidates.InsertOneAsync(offerIceCandidate);

        public Task InsertAnswerCandidate(IceCandidate answerIceCandidate) =>
            _answerCandidates.InsertOneAsync(answerIceCandidate);
    }
}