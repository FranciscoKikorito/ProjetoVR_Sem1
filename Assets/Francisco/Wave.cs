public class Wave
{
    public int waveNum;
    public int maxEnemyNum;
    public float enemyHealthIncrease;
    public float enemyHealth;
    public int enemyNumberIncrease;

    public Wave(int waveNum, float enemyHealth, int maxEnemyNum)
    {
        this.waveNum = waveNum;
        this.enemyHealth = enemyHealth;
        this.enemyHealthIncrease = enemyHealth;
        this.maxEnemyNum = maxEnemyNum;
    }
}
