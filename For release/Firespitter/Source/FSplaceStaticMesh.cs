using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

class FSplaceStaticMesh : PartModule
{
    public GameObject structure;
    public int structureID = 0;
    FSdebugMessages db = new FSdebugMessages(true, FSdebugMessages.OutputMode.screen, 5f);

    [KSPField]
    public string meshName = "Firespitter/Parts/Command/FS_bomberCockpit/model";

    [KSPEvent(guiActive = true, guiName = "place structure")]
    public void placeMeshEvent()
    {
        placeMeshEvent(meshName);
    }

    public void placeMeshEvent(string modelName)
    {
        structure = GameDatabase.Instance.GetModel(meshName);        
        structure.transform.position = part.transform.position + new Vector3(0f, 0f, 2f);
        Rigidbody newRigidBody = structure.AddComponent<Rigidbody>();
        newRigidBody.mass = 1.0f;
        newRigidBody.drag = 0.05f;
        newRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        FSstaticMeshCollisionHandler colliderHandler = structure.AddComponent<FSstaticMeshCollisionHandler>();
        colliderHandler.thisCollider = structure.collider;
        structure.AddComponent<physicalObject>();

        structure.SetActive(true);        
        db.debugMessage("FS: mesh pos == " + structure.transform.position);
        db.debugMessage("FS: part pos == " + part.transform.position);
    }
}

class FSstaticMeshCollisionHandler : MonoBehaviour
{
    public Collider thisCollider;
    void OnTriggerEnter(Collider other)
    {
        FSdebugMessages.Post("hit " + other.name, true, 5f);
    }
}