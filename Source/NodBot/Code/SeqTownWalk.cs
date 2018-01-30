using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class SeqTownWalk
    {

        private Logger mLogger;
        private ImageAnalyze mImageAnalyze;
        private NodiatisInput mInput;

        private bool mTravelNorth = false;

        /// <summary>
        /// These points represent the min and max values to determine where the
        /// user is in accordance to the specified towns.
        /// </summary>
        private Point mTown5Min = new Point(329, 322);
        private Point mTown5Max = new Point(429, 352);
        private Point mTown4Min = new Point(347, 315);
        private Point mTown4Max = new Point(447, 315);

        /// <summary>
        /// This is the constructor fo the Town Walking Sequence.
        /// 
        /// </summary>
        /// <param name="aLogger"></param>
        public SeqTownWalk(Logger aLogger)
        {
            mLogger = aLogger;
            mImageAnalyze = new ImageAnalyze(mLogger);
            mInput = new NodiatisInput(mLogger);
        }

        /// <summary>
        /// This function decides which direction to move based on the specified point
        /// and which direction we are trying to travel towards.
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="aTownSeen"></param>
        private void makeCorrectMove(Point p, bool aTownSeen)
        {

            if (mTravelNorth)
            {
                if (aTownSeen)
                {
                    if (p.Y < mTown5Min.Y) mInput.moveUp();
                    else if (p.Y >= mTown5Max.Y) mInput.moveDown();
                    else
                    {
                        if (p.X >= mTown5Max.X) mInput.moveRight();
                        else if (p.X <= mTown5Min.X) mInput.moveLeft();
                        else mTravelNorth = !mTravelNorth;
                    }
                } else
                {
                    mInput.moveUp();
                }
            }
            else
            {
                if (aTownSeen)
                {
                    if (p.Y < mTown4Min.Y) mInput.moveUp();
                    else if (p.Y > mTown4Min.Y) mInput.moveDown();
                    else
                    {
                        if (p.X <= mTown4Min.X) mInput.moveLeft();
                        else if (p.X >= mTown4Max.X) mInput.moveRight();
                        else mTravelNorth = !mTravelNorth;
                    }
                }
                else
                {
                    mInput.moveDown();
                }
            }
        }

        /// <summary>
        /// This function runs the town walking sequence between town4 and town5.
        /// 
        /// </summary>
        /// <param name="aCt"></param>
        /// <returns></returns>
        public async Task Start(CancellationToken aCt)
        {
            bool seenT4 = false;

            Point? aTown = null;
            while (true)
            {
                aCt.ThrowIfCancellationRequested();

                if (mTravelNorth)
                {
                    aTown = mImageAnalyze.getMatchCoord(NodImages.Town5, NodImages.CurrentSS);

                    if (aTown == null) makeCorrectMove(new Point(), false);
                    else makeCorrectMove(aTown.Value, true);
                }else
                {
                    
                    aTown = mImageAnalyze.getMatchCoord(NodImages.Town4, NodImages.CurrentSS);
                    if (!seenT4) seenT4 = aTown != null;

                    if (seenT4 && aTown == null)
                    {
                        mTravelNorth = !mTravelNorth;
                        seenT4 = false;
                        continue;
                    }

                    if (aTown == null) makeCorrectMove(new Point(), false);
                    else makeCorrectMove(aTown.Value, true);
                }

                await checkForCombat();
            }
        }

        /// <summary>
        /// This function checks for combat after moving, and if combat has started
        /// attacks and exits accordingly to continue town walking.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task checkForCombat()
        {
            await delay(1750);

            if (mImageAnalyze.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
            {
                mInput.SettingsAttack();
                await delay(1000);

                while (!mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS)) ;

                mInput.Exit();
                await delay(3500);
            }

        }

        /// <summary>
        /// This function delays the town walking task by the specified time (ms).
        /// 
        /// </summary>
        /// <param name="aTime"></param>
        /// <returns></returns>
        private async Task delay(int aTime)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(aTime);
            });
        }
    }
}
