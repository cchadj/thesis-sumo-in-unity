namespace UtilityAI {
    public class MultiplyAllQualifier : Qualifier {
        public override float Score(IContext context) {
            var totalScore = 1f*scorers.Count;
            foreach (var scorer in scorers) {
                var score = scorer.Score(context);
                //UnityEngine.Debug.Log("Score is:" + score);

                totalScore *= score;

                if (AIDebuggingHook.debugger != null) {
                    AIDebuggingHook.debugger.ContextualScorer(scorer, score);
                }
            }
            //UnityEngine.Debug.Log("Score is:"+totalScore);
            return totalScore;
        }
    }
}