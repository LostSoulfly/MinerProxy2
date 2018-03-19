/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

namespace MinerProxy2.Pools
{
    public class PoolManager
    {
        //not sure how I want to handle this.
        //Would like to init a PoolManager for each coin/port
        //then have the PoolManager handle the pools (poolitem) for that coin
        //and possibly have the option for multiple pools for the same coin
        //with a maxMiner value being reached, a new connection to the pool (or a different pool) is made
        //This may have to be tied into the PoolClient more tightly.
        //Could also initialize the pools HERE and pass them to the PoolClient/MinerServer instances
    }
}