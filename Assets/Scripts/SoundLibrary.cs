using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Header("Player")]
    public AudioClip[] clawSlash;
    public AudioClip jump;
    public AudioClip hurt;
    public AudioClip maskChange;
    public AudioClip gameOver;
    public AudioClip tentacleSlide;
    public AudioClip tentacleWhip;

    [Header("Melee Enemy")]
    public AudioClip meleeAttack;
    public AudioClip[] meleeDeath;

    [Header("Ranged Enemy")]
    public AudioClip rangedShoot;
    public AudioClip rangedMeleeDefend;
    public AudioClip[] rangedDeath;


    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
}
