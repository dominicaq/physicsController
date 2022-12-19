using UnityEngine;
using System.Collections;

public abstract class State : ScriptableObject
{
    protected CharacterMotor character;
    public float acceleration = 1.0f;
    public bool isKinematic = false;
    public abstract void Tick();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Jump(ref Vector3 velocity) {}
    public virtual void Crouch(ref IEnumerator routine) {}

    // Ported from the Quake-III engine
    // https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c
    public void Accelerate(Vector3 wishdir, ref Vector3 velocity, float wishspeed, float accel) {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if(addspeed <= 0)
            return;

        accelspeed = accel * Time.deltaTime * wishspeed;
        if(accelspeed > addspeed)
            accelspeed = addspeed;

        velocity.x += accelspeed * wishdir.x;
        velocity.z += accelspeed * wishdir.z;
    }

    public virtual Vector3 ProcessRawInput(Transform player, Vector3 wishDir) {
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);
        return player.TransformDirection(wishDir);
    }

    public virtual void RotatePlayer(Vector3 rotateSource, Transform player) {
        // Rotate with camera
        Vector3 yRot = new Vector3(0, rotateSource.y, 0);
        player.eulerAngles = yRot;
    }
    
    public void Init(CharacterMotor character) {
        this.character = character;
    }
}