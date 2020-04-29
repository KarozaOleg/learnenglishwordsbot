using LearnEnglishWordsBot.Interfaces;
using LearnEnglishWordsBot.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LearnEnglishWordsBot.Controllers
{
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        readonly ILearnSetRepository _learnSetRepository;
        readonly IWordsRepository _wordsRepository;

        public UploadController(
            ILearnSetRepository learSetRepository,
            IWordsRepository wordsRepository)
        {
            _learnSetRepository = learSetRepository;
            _wordsRepository = wordsRepository;
        }

        [HttpPost("learnset/add")]
        public ActionResult<string> Post([FromBody]UploadLearnSet uploadLearnSet)
        {
            _learnSetRepository.SetAdd(uploadLearnSet.Name, out int idLearnSet);
            return StatusCode(200, $"Success created LearnSet with name:{uploadLearnSet.Name} and id:{idLearnSet}");
        }

        [HttpPost("word/upload")]
        public ActionResult<string> Post([FromBody]UploadWords uploadWords)
        {
            if (_learnSetRepository.GetIsExist(uploadWords.IdLearnSet) == false)
            {
                return StatusCode(404, $"LearnSet with id:{uploadWords.IdLearnSet} doesn't exist");
            }

            var added = 0;
            var errors = new List<string>();
            for (int i = 0; i < uploadWords.Words.Length; i++)
            {
                _wordsRepository.SetAdd(uploadWords.Words[i].Russian, uploadWords.Words[i].English, out int idWord);
                _learnSetRepository.SetAddWord(idWord, uploadWords.IdLearnSet, out bool isAlreadyExist);
                if (isAlreadyExist)
                    errors.Add($"Word \"{uploadWords.Words[i].English}\" is already exist in LearnSet with id:{uploadWords.IdLearnSet}");
                else
                    ++added;
            }

            var answer = new StringBuilder(200);
            for (int i = 0; i < errors.Count; i++)            
                answer.Append(errors[i]).Append(Environment.NewLine);

            answer.Append($"Success processed:{uploadWords.Words.Length} words, added:{added} for LearnSet with id:{uploadWords.IdLearnSet}");
            return StatusCode(200, answer.ToString());
        }
    }
}
