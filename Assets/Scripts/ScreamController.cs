using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamController : NetworkBehaviour
{
    public AudioClip screamSound; // ���� �����

    public AudioSource screamSource;
    // public float screamRadius = 5f; // ������ �������� �����
    //  public int screamDamage = 20; // ����
    public override void OnStartClient()
    {
        base.OnStartClient();
        if(base.IsOwner)
        {
            screamSource.clip = screamSound;
        }
        else
        {
            Debug.Log("������ ����� ���� ������ ����");
            return;
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("�������");
            ScreamServer();
        }
    }
    [ServerRpc]
    public void ScreamServer()
    {
        Scream();
    }
    [ObserversRpc]
    public void Scream()
    {
        screamSource.clip = screamSound;
        screamSource.Play();
    }
}
