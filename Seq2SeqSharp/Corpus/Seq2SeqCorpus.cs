﻿using AdvUtils;
using Seq2SeqSharp.Tools;
using Seq2SeqSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqSharp.Corpus
{
    public class Seq2SeqCorpus : ParallelCorpus<Seq2SeqCorpusBatch>
    {

        public Seq2SeqCorpus(string corpusFilePath, string srcLangName, string tgtLangName, int batchSize, int shuffleBlockSize = -1, int maxSrcSentLength = 32, int maxTgtSentLength = 32, ShuffleEnums shuffleEnums = ShuffleEnums.Random)
            :base (corpusFilePath, srcLangName, tgtLangName, batchSize, shuffleBlockSize, maxSrcSentLength, maxTgtSentLength, shuffleEnums: shuffleEnums)
        {

        }

        /// <summary>
        /// Build vocabulary from training corpus
        /// </summary>
        /// <param name="vocabSize"></param>
        public (Vocab, Vocab) BuildVocabs(int vocabSize = 45000, bool sharedVocab = false)
        {
            foreach (var sntPairBatch in this)
            {
                CorpusBatch.CountSntPairTokens(sntPairBatch.SntPairs);
            }

            CorpusBatch.ReduceSrcTokensToSingleGroup();
            if (sharedVocab)
            {
                CorpusBatch.MergeTokensCountSrcTgt(0, 0);
            }

            (var srcVocabs, var tgtVocabs) = CorpusBatch.GenerateVocabs(vocabSize);           
            return (srcVocabs[0], tgtVocabs[0]);         
        }
    }
}
