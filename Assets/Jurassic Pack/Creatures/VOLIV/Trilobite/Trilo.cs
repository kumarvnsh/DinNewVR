using UnityEngine;

public class Trilo : Creature
{
	public Transform Root,Spine0,Spine1,Spine2,Spine3,Spine4,Neck0,Neck1,Neck2,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Slip,Bite,Swallow,MedSplash;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		ManageCollision(col, source, Bite, Hit_jaw, Hit_head, Hit_tail);
	}
	void PlaySound(string name, int time)
	{
		if(time==currframe && lastframe!=currframe)
		{
			switch (name)
			{
			case "Swim": source[1].pitch=Random.Range(0.75f, 1.0f);
				if(isOnWater && isOnGround) source[1].PlayOneShot(MedSplash, 0.1f);
				else if(isOnGround && !isInWater) source[1].PlayOneShot(Slip, 0.1f);
				else if(isOnWater) source[1].PlayOneShot(Waterflush,  0.1f);
				lastframe=currframe; break;
			case "Bite": source[0].pitch=Random.Range(0.25f, 0.5f); source[0].PlayOneShot(Bite, 0.5f);
				lastframe=currframe; break;
      case "Flush": source[1].pitch=Random.Range(1.25f, 1.5f); source[1].PlayOneShot(Waterflush, 0.25f);
			lastframe=currframe; break;
			case "Food": source[0].pitch=Random.Range(0.25f, 0.5f); source[0].PlayOneShot(Swallow,  0.25f);
				lastframe=currframe; break;
			case "Die":source[0].pitch=Random.Range(0.5f, 0.75f); source[0].PlayOneShot(Swallow, 0.5f);
				lastframe=currframe; isDead=true; break;
			}
		}
	}
	
	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
		onJump=false; onAttack=false; isOnLevitation=false; isConstrained=false; onReset=false;
		Vector3 dir=-Neck0.up;

		if(useAI && health!=0) { AICore(1, 2, 0, 0, 3, 0, 0); }// CPU
		else if(health!=0) { GetUserInputs(1, 2, 0, 0, 3, 0, 0); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
		if(isInWater)
		{
      body.drag=1; body.angularDrag=1; 
      if(health!=0)
			{
        anm.SetBool("OnGround", false);
			  pitch=Mathf.Lerp(pitch, anm.GetFloat("Pitch")*90f, ang_T);
			  if(anm.GetInteger("Move").Equals(-1)) Move(-dir,30);
        else if(anm.GetInteger("Move").Equals(1)) Move(dir,30);
				else if(anm.GetInteger("Move").Equals(10)) Move(Head.right.normalized,30);
				else if(anm.GetInteger("Move").Equals(-10)) Move(-Head.right.normalized,30);
				else if(!anm.GetInteger("Move").Equals(0)) Move(dir,60);
        else Move(Vector3.zero);
        isOnLevitation=true;
			}
      if(isOnWater) ApplyGravity();
		}
		else if(isOnGround) { body.drag=4; body.angularDrag=4; anm.SetBool("OnGround", true); ApplyYPos(); }
    else
    {
      if(health!=0) { Move(Vector3.zero); pitch=Mathf.Lerp(pitch, anm.GetFloat("Pitch")*90f, ang_T); }
      onJump=true; body.drag=1f; body.angularDrag=1f; ApplyGravity();
    }

		//Stopped
		if(OnAnm.IsName("Trilo|Die") | OnAnm.IsName("Trilo|DieOnGround"))
		{
			onReset=true;
			if(!isDead) PlaySound("Die", 2);
		}
		
		//Forward
		else if(OnAnm.IsName("Trilo|Swim"))
		{
			PlaySound("Swim", (int) currframe);
		}
		
		//Backward
		else if(OnAnm.IsName("Trilo|Swim-"))
		{
			PlaySound("Swim", (int) currframe);
		}
    //Strafe-
    else if(OnAnm.IsName("Trilo|SwimStrafe-"))
		{
			PlaySound("Swim", (int) currframe);
		}
    //Strafe+
    else if(OnAnm.IsName("Trilo|SwimStrafe+"))
		{
			PlaySound("Swim", (int) currframe);
		}
		
		//Running
		else if(OnAnm.IsName("Trilo|SwimFast") )
		{
			PlaySound("Swim",  (int) currframe);
		}

    //Impulse
		else if(OnAnm.IsName("Trilo|SwimInpulse") )
		{
			if(isInWater&&OnAnm.normalizedTime<0.4) { PlaySound("Flush", 2);  Move(transform.forward, 80); }
			PlaySound("Swim",  (int) currframe);
		}
		
		//Attack
		else if(OnAnm.IsName("Trilo|Atk") | OnAnm.IsName("Trilo|SwimAtk"))
		{
			if(OnAnm.IsName("Trilo|SwimAtk")) { PlaySound("Flush", 2); PlaySound("Bite", 7); onAttack=true; }
			else { PlaySound("Flush", 1);  PlaySound("Bite", 10); onAttack=true; }
			PlaySound("Swim",  (int) currframe);
		}
		
		//On Ground
		else if(OnAnm.IsName("Trilo|OnGround"))
		{
			Move(transform.forward, 25);
			PlaySound("Swim", 5); PlaySound("Swim", 10);
		}
		
		//Various
		else if(OnAnm.IsName("Trilo|Eat")) { onReset=true; isConstrained=true; PlaySound("Food", 2); }
		else if(OnAnm.IsName("Trilo|Die-")) isDead=false;

    RotateBone(IkType.None, 30f, 20f, false);
	}
	
  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
    Root.rotation*= Quaternion.Euler(Mathf.Clamp(-pitch, -90f, 90f), roll*6f, 0);
		Neck0.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Neck1.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Neck2.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Head.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Spine0.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Spine1.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Spine2.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Spine3.rotation*= Quaternion.Euler(spineY, 0, spineX);
    Spine4.rotation*= Quaternion.Euler(spineY, 0, spineX);
		Tail0.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail1.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail2.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail3.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail4.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail5.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail6.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail7.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
		Tail8.rotation*= Quaternion.Euler(-spineY, 0, -spineX);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(0, lastHit, 0);
		//Check for ground layer
		GetGroundPos(IkType.None);
	}
}



