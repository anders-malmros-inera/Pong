Unity Pong - Quick Setup

Files added:
- `Assets/Scripts/GameManager.cs`
- `Assets/Scripts/Ball.cs`
- `Assets/Scripts/Paddle.cs`
- `Assets/Scripts/UIManager.cs`

Quick Scene Setup (in Unity Editor):

1. Create a new 2D Scene.

2. Camera:
   - Use an Orthographic `Main Camera` with `Size` ~5 (default).

3. Paddles:
   - Create two `Sprite` objects (e.g., a white rectangle) and name them `PaddleLeft` and `PaddleRight`.
   - Attach `Paddle.cs` to each.
   - For `PaddleLeft`: set `isLeft = true`, for `PaddleRight`: `isLeft = false`.
   - Add `Rigidbody2D` (set `Body Type` to Kinematic) and `BoxCollider2D` to each.
   - Position `PaddleLeft` at x = -8, `PaddleRight` at x = 8 (adjust to your camera size).

4. Ball:
   - Create a `Sprite` (circle) named `Ball` and attach `Ball.cs`.
   - Add `Rigidbody2D` (set `Gravity Scale` = 0) and `CircleCollider2D`.

5. Goals:
   - Create two empty `GameObject`s with `BoxCollider2D` components set as `Is Trigger`.
   - Name them `GoalLeft` (place to left of left paddle) and `GoalRight` (place to right of right paddle).
   - Tag them with `GoalLeft` and `GoalRight` respectively (create tags in Tag Manager).

6. UI:
   - Create a `Canvas` -> `Text` elements for left and right scores.
   - Add `UIManager.cs` to an empty GameObject (or the Canvas) and assign the left/right `Text` references.

7. GameManager:
   - Create an empty GameObject named `GameManager` and attach `GameManager.cs`.
   - Assign the `Ball` reference and the `UIManager` instance to the `GameManager` inspector fields.

8. Tags:
   - Tag the paddles with tag `Paddle`.

9. Play:
   - Press `Play` in the editor. The ball will spawn and move; when it hits a goal, a point is awarded and the ball resets.

Notes & tweaks:
- Adjust speeds in the inspector for `Ball` and `Paddle` for desired gameplay.
- The `Paddle` script clamps paddles based on the camera's orthographic size; adjust paddle sprite scale accordingly.
- This is a minimal starting point — you can add menus, sound, better AI, and polish later.
