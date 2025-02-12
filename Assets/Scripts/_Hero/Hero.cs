﻿using UnityEngine;

public class Hero : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;				// The fastest the player can travel in the x axis.
	[SerializeField] float jumpForce = 400f;			// Amount of force added when the player jumps.

	[Range(0, 1)]
	[SerializeField] float slideDecel = 0.5f;		// How quickly the slide decreases in velociy;
	float slideTParam = 0f;
	float slideLerped = 0f;

	[SerializeField] bool enableDoubleJump = false;		// Whether or not a player can double jump;
	bool doubleJump;									// Declare double jump bool
	[SerializeField] bool airControl = false;			// Whether or not a player can steer while jumping;
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character

	Transform groundCheck;								// A position marking where to check if the player is grounded.
	float groundedRadius = .2f;							// Radius of the overlap circle to determine if grounded
	bool grounded = false;								// Whether or not the player is grounded.
	Transform ceilingCheck;								// A position marking where to check for ceilings
	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	Animator anim;										// Reference to the player's animator component.


    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Set the vertical animation
		anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);

		//Reset DoubleJump (if enabled)
		if (enableDoubleJump && grounded) {
				doubleJump = false;
		} else if (!enableDoubleJump) {
				doubleJump = true;
		} 
	}


	public void Move(float move, bool crouch, bool jump)
	{


		// If crouching, check to see if the character can stand up
		if(!crouch && anim.GetBool("Crouch"))
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if( Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true;
		}

		// Set whether or not the character is crouching in the animator
		anim.SetBool("Crouch", crouch);

		//only control the player if grounded or airControl is turned on
		if(grounded || airControl)
		{			
			// Reduce the speed if crouching by the crouchSpeed multiplier
			if (crouch) { 

				if ((slideTParam < 1) && !Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround)) { // Makes sure they can slip out if stuck in a crouch
					slideTParam += Time.deltaTime * (slideDecel * 2); // This will increment tParam based on Time.deltaTime multiplied by a speed multiplier
					slideLerped = Mathf.Lerp(1, 0, slideTParam); // fades from 1 to 0 (decelerates)
				}

				move =  move * slideLerped;

			} else {
				// Reset the Slide Variables for the next slide
				slideLerped = 1;
				slideTParam = 0f;
			}

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(move));

			// Move the character
			GetComponent<Rigidbody2D>().velocity = new Vector2(move * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			
			// If the input is moving the player right and the player is facing left...
			if(move > 0 && !facingRight)
				// ... flip the player.
				Flip();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if(move < 0 && facingRight)
				// ... flip the player.
				Flip();
		}

		// If the player should jump...
		if ((grounded || !doubleJump) && jump) {
			// Add a vertical force to the player.
			anim.SetBool("Ground", false);
			
			//zero out our current velocity
			GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
			
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));
			
			if(!grounded && doubleJump == false)
			{
				doubleJump = true;
			}
		}
	}

	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
