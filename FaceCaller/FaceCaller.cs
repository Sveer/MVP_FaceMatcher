using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceCaller
{
   
    /// <summary>
    /// Класс шлет в ажур всякие фоты
    /// </summary>
    public class FaceCaller
    {

        public class MVP
        {
            public string photoUrl { get; set; }
            public string name { get; set; }

        }

        public class MVPcandidate:MVP
        {
            public double confidence { get; set; }
        }


        const string subscriptionKey = "910cfff45acb4c558848d0dc6d98835e";
        const string personGroupName = "mvplist";
        Microsoft.ProjectOxford.Face.FaceServiceClient _client;

        public FaceCaller()
        {
            _client = new Microsoft.ProjectOxford.Face.FaceServiceClient(subscriptionKey);
        }


        public async Task<MVPcandidate> FindSimilarMVPAsync(string testFaceUrl)
        {
            var faces = await _client.DetectAsync(testFaceUrl);
            var faceIds = faces.Select(face => face.FaceId).ToArray();
            var faceResults = await _client.IdentifyAsync(personGroupName, faceIds);

            if (faceResults.Length > 0)
            {
                if (faceResults[0].Candidates.Length>0)
                {
                    var confidence = faceResults[0].Candidates[0].Confidence;
                    var person = await _client.GetPersonAsync(personGroupName, faceResults[0].Candidates[0].PersonId);
                    return  new MVPcandidate()
                    {
                        name = person.Name,
                        confidence = confidence,
                        photoUrl = person.UserData
                    
                    };
                }
            }

            return null;
        }

        public Task<Person[]> GetPersonsAsync(byte photo)
        {
            return _client.ListPersonsAsync(personGroupName);
        }

        public async Task CreateGroupAsync()
        {
            await _client.CreatePersonGroupAsync(personGroupName, personGroupName);
        }

        public async Task<int> FillMVPPhotos(string csv)
        {
            MVP hz = new MVP();
            using (var reader = new CsvHelper.CsvReader(new StringReader(csv)))
            {
                var mvps = reader.EnumerateRecords<MVP>(hz);
                return await FillMVPPhotos(mvps);
            }

        }


        public async Task<int> FillMVPPhotos(IEnumerable<MVP> mvps)
        {
            int counter = 0;
            foreach (var face in mvps)
            {
                try
                {
                    var cpResult = await _client.CreatePersonAsync(face.name, face.name, face.photoUrl);
                    Guid personId = cpResult.PersonId;

                    var addfaceResult = await _client.AddPersonFaceAsync(personGroupName, personId, face.photoUrl);
                    if (addfaceResult.PersistedFaceId != Guid.Empty)
                        counter++;
                }
                catch(Exception ex) {
                    //Bad code detected :)
                }
            }
            return counter;

        }
    }
}
