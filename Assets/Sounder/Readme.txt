Sounder Readme

Sounder creates parameterized sound effects. The effects can built to .wav file or built to Unity's AudioClip class.

To create an effect click Assets->Create->Sounder Effect on the menu. Or right click in the project window and go Create->Sounder Effect.

Sounder Effects are ScriptableObjects and can be used in your scripts just like any Object.

See the script PlayExample in the ExampleScene for some samples on how to use Sounder.

Sounder provides a simple Object pool for AudioSources to play SounderEffects. It can also be used to play any other sounds in your project with:
	Sounder.Player.PlayClip(yourClip);
