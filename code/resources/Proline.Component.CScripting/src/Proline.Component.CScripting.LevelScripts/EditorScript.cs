﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Proline.CScripting.Framework;

namespace Proline.Classic.Scripts
{
    public class EditorScript : ScriptInstance
    {
        private Camera _cam;
        private RaycastResult _raycastResult; 
        private float _cameraSensitivity;
        private float _movementSpeed;
        private PointF _st;
        private List<Entity> _garbage;
        private float _cx;
        private float _cy;
        private PointF _et;
        private Vector2[] _points;
        private Vector3[] _worldPos;
        private bool _multiSelectEnabled;
        private int _buttonHeld2;
        private Vector2[] _p;

        public override async Task Execute(params object[] args)
        {
            _cam = World.CreateCamera(CitizenFX.Core.Game.PlayerPed.Position, new Vector3(1, 0, 0), 90);
            World.RenderingCamera = _cam;
            CitizenFX.Core.Game.PlayerPed.IsVisible = false;
            CitizenFX.Core.Game.PlayerPed.IsPositionFrozen = true;
            CitizenFX.Core.Game.PlayerPed.IsInvincible = true;
            CitizenFX.Core.Game.PlayerPed.CanRagdoll = false;
            CitizenFX.Core.Game.PlayerPed.IsCollisionEnabled = false;
            Screen.Hud.IsRadarVisible = false;

            _worldPos = new Vector3[0];
            _garbage = new List<Entity>();
            _cameraSensitivity = 10f;
            _movementSpeed = 5f;

            while (Stage == 0)
            {
                CitizenFX.Core.Game.PlayerPed.Position = _cam.Position; 

                EditorControls();
                if (CitizenFX.Core.Game.IsControlPressed(0, Control.Aim))
                {
                    CameraRotation();
                    CameraMovement();
                }
                else if (CitizenFX.Core.Game.IsControlJustReleased(0, Control.Aim))
                {
                    API.SetCursorLocation(0.5f, 0.5f);
                }
                else
                {
                    API.ShowCursorThisFrame();
                    _cx = CitizenFX.Core.Game.GetControlNormal(0, Control.CursorX);
                    _cy = CitizenFX.Core.Game.GetControlNormal(0, Control.CursorY);
                    //World.DrawLine(_cam.Position, d, Color.FromArgb(255, 0, 0));
                    //World.DrawMarker(MarkerType.DebugSphere, _raycastResult.HitPosition, new Vector3(0, 0, 0),
                    //    new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.FromArgb(150, 255, 255, 255));

                }

                foreach (var item in _worldPos)
                {
                    World.DrawMarker(MarkerType.DebugSphere, item, new Vector3(0, 0, 0),
                        new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.FromArgb(150, 255, 255, 255));
                }

                _garbage = _garbage.Where(e => e.Exists()).ToList();
                foreach (var item in _garbage)
                {
                    var x =
                       $"{item.Handle}\n" +
                       $"{item.Model.Hash}\n" +
                       $"{item.Health}\n" +
                       $"{item.Position.ToString()}";
                    var d = item.Position + new Vector3(0, 0, (item.Model.GetDimensions().Z * 0.8f));
                    World.DrawMarker(MarkerType.DebugSphere, item.Position, new Vector3(0, 0, 0),
                        new Vector3(0, 0, 0), new Vector3(0.2f, 0.2f, 0.2f), Color.FromArgb(150, 255, 0, 0));
                    //ComponentAPI.DrawEntityBoundingBox(item.Handle, 125, 0, 0, 100);
                    //ComponentAPI.DrawDebugText3D(x, d, 3f, 0);
                }

                //if (_startPos != Vector3.Zero && _endPos != Vector3.Zero)
                //{
                //    //API.DrawBox(_startPos.X, _startPos.Y, _startPos.Z, _endPos.X, _endPos.Y, _endPos.Z, 255, 255, 255, 125);
                //    ExampleAPI.DrawBoundingBox(_startPos, _endPos, 255, 255, 255, 125);
                //}

                //if (_vertextPoints.Length > 0)
                //{
                //    //ExampleAPI.DrawBoundingBoxFromPoints(_vertextPoints, 255, 255, 255, 125);
                //}
                await BaseScript.Delay(0);
            }
            CitizenFX.Core.Game.PlayerPed.IsVisible = true;
            CitizenFX.Core.Game.PlayerPed.IsPositionFrozen = false;
            CitizenFX.Core.Game.PlayerPed.IsInvincible = false;
            CitizenFX.Core.Game.PlayerPed.CanRagdoll = true;
            CitizenFX.Core.Game.PlayerPed.IsCollisionEnabled = true;
            Screen.Hud.IsRadarVisible = true;
            World.RenderingCamera = null;
        }

        private void EditorControls()
        {
            API.BlockWeaponWheelThisFrame();
            CitizenFX.Core.Game.DisableControlThisFrame(0, Control.Aim);
            if (CitizenFX.Core.Game.IsControlJustPressed(0, Control.Aim))
            {

            }

            CitizenFX.Core.Game.DisableControlThisFrame(0, Control.Attack);
            if (CitizenFX.Core.Game.IsControlJustPressed(0, Control.Attack))
            {
                _multiSelectEnabled = false;
                _buttonHeld2 = 0;
                foreach (var item in _garbage)
                {
                    item.Opacity = 255;
                }
                _st = new PointF(_cx, _cy);
            }
            else if (CitizenFX.Core.Game.IsControlJustReleased(0, Control.Attack))
            {

                var han = new List<int>();
                //ExampleAPI.GetNearbyEntities(out var handles);
                han.AddRange(null);


                if(!CitizenFX.Core.Game.IsControlPressed(0, Control.VehicleFlyThrottleUp))
                    Reset();
                if (!_multiSelectEnabled)
                {
                    Vector3 d = Vector3.Right;//ComponentAPI.ScreenRelToWorld(_cam.Position, _cam.Rotation, new Vector2(_cx, _cy), out var dir);
                    Vector3 dir = Vector3.Right;//ComponentAPI.ScreenRelToWorld(_cam.Position, _cam.Rotation, new Vector2(_cx, _cy), out var dir);
                    _raycastResult = World.Raycast(d, dir, 1000f, IntersectOptions.Everything);
                    if (_raycastResult.DitHit)
                    {
                        if (_raycastResult.DitHitEntity)
                        {
                            if (!Exists(_raycastResult.HitEntity))
                                _garbage.Add(_raycastResult.HitEntity);
                        }
                        else
                        {
                            //Reset();
                            var entityes = han.Select(e => Entity.FromHandle(e)).Where(e=>e != null).ToArray();
                            var closest = World.GetClosest(_raycastResult.HitPosition, entityes);
                            if(closest != null)
                            { 
                                if (World.GetDistance(_raycastResult.HitPosition, closest.Position) < 1f)
                                {
                                    if (!Exists(_raycastResult.HitEntity))
                                    {
                                        _garbage.Add(_raycastResult.HitEntity);
                                    }
                                }
                            } 
                        }
                    }
                }
                else
                {
                    var st = new Vector2(_st.X, _st.Y);
                    var et = new Vector2(_cx, _cy);

                    _points = new Vector2[]
                    {
                        st,
                        new Vector2(et.X, st.Y ),
                        new Vector2(st.X, et.Y ),
                        et,
                    };

                    _worldPos = new Vector3[_points.Length];
                    _p = new Vector2[_points.Length];

                    //for (int i = 0; i < _points.Length; i++)
                    //{
                    //    var raycast = World.Raycast(ComponentAPI.ScreenRelToWorld(_cam.Position, _cam.Rotation, _points[i], out var dir), dir, 1000f, IntersectOptions.Everything);
                    //    _worldPos[i] = raycast.HitPosition;
                    //    _p[i] = new Vector2(raycast.HitPosition.X, raycast.HitPosition.Y);
                    //}

                    foreach (var item in han)
                    {
                        var entity = Entity.FromHandle(item);
                        if (entity == null) continue;
                        var position = entity.Position;
                        //if (position.X > min.X && position.Y > min.Y && position.Z > min.Z && position.X < max.X && position.Y < max.Y && position.Z < max.Z)
                        if (PointInRectangle(_p[0], _p[1], _p[2], _p[3], new Vector2(position.X, position.Y)) && entity != CitizenFX.Core.Game.PlayerPed && !Exists(entity))
                        {
                            _garbage.Add(entity);
                            LogDebug(position);
                            LogDebug(item);
                        }
                    }
                }


                _buttonHeld2 = 0;
            }
            else if (CitizenFX.Core.Game.IsControlPressed(0, Control.Attack))
            {
                _buttonHeld2++;
                if(_buttonHeld2 > 10)
                {
                    //ComponentAPI.DrawDebug2DBox(_st, new PointF(_cx, _cy), Color.FromArgb(100, 100, 100, 100));
                    _multiSelectEnabled = true;
                }
            }

            if (CitizenFX.Core.Game.IsControlJustPressed(0, Control.Context))
            {  
                foreach (var item in _garbage)
                {
                    StartNewScript("BlowUp", item.Handle);
                }
            }

            if (CitizenFX.Core.Game.IsControlJustPressed(0, Control.FrontendPause))
            {
                World.RenderingCamera = null;
                CitizenFX.Core.Game.PlayerPed.IsVisible = true;
                CitizenFX.Core.Game.PlayerPed.IsInvincible = false;
                CitizenFX.Core.Game.PlayerPed.CanRagdoll = true;
                CitizenFX.Core.Game.PlayerPed.IsPositionFrozen = false;
                Screen.Hud.IsRadarVisible = true;
            }
        }

        private void Reset()
        {
            _garbage.Clear();
            foreach (var item in _garbage)
            {
                item.Opacity = 255;
            }
        }

        private bool Exists(Entity entity)
        {
            foreach (var item in _garbage)
            {
                if (item == entity)
                    return true;
            }
            return false;
        }

        private bool P(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        { 
            var apd = a + p + d;
            var dpc = d + p + c;
            var cpb = c + p + b;
            var pba = p + b + a;

            return (p.X > a.X && p.X < d.X && p.Y > a.Y && p.Y < d.Y);
        }

        private bool P(Vector2 a, Vector2 b, Vector2 m)
        {
            var ab = new Vector2(b.X - a.X, b.Y - a.Y);
            var am = new Vector2(m.X - a.X, m.Y - a.Y);
            var x = am * ab;
            return x.X < 0 && x.Y < 0;
        }

        float isLeft(Vector2 P0, Vector2 P1, Vector2 P2)
        {
            return ((P1.X - P0.X) * (P2.Y - P0.Y) - (P2.X - P0.X) * (P1.Y - P0.Y));
        }
        bool PointInRectangle(Vector2 X, Vector2 Y, Vector2 Z, Vector2 W, Vector2 P)
        {
            return (isLeft(X, Y, P) > 0 && isLeft(Y, Z, P) > 0 && isLeft(Z, W, P) > 0 && isLeft(W, X, P) > 0);
        }

        private bool isInsideRect(Vector2 x1, Vector2 x2, Vector2 px)
        {
            return px.X >= x1.X && px.X <= x2.X && px.Y >= x1.Y && px.Y <= x2.Y;
        }

        private bool Greater(Vector2 p, Vector2 pba)
        {
            return p.X > pba.X && p.Y > pba.Y;
        }

        private void CameraRotation()
        {
            var x = CitizenFX.Core.Game.GetControlNormal(0, Control.LookLeftRight);
            var y = CitizenFX.Core.Game.GetControlNormal(0, Control.LookUpDown);

            x *= -1;
            y *= -1;
            var z = _cam.Rotation.X + y * _cameraSensitivity;
            //LogDebug(z);
            if (z > 89)
            {
                z = 89;
            }
            else
            if (z < -89)
            {
                z = -89;
            }
            _cam.Rotation = new Vector3(z, 0,
                _cam.Rotation.Z + x * _cameraSensitivity);
        }

        private void CameraMovement()
        {
            //if (Game.IsControlPressed(0, Control.SniperZoomInOnly))
            //{
            //    if (_movementSpeed >= 1000) return;
            //    _movementSpeed += 2f;
            //}
            //else if (Game.IsControlPressed(0, Control.SniperZoomOutOnly))
            //{
            //    if (_movementSpeed <= 0) return;
            //    _movementSpeed -= 2f;
            //}

            _movementSpeed = 25f;
            // Up and Down
            if (CitizenFX.Core.Game.IsControlPressed(0, Control.VehicleFlyThrottleUp))
                _movementSpeed = 50f;
            //_cam.Position -= _cam.ForwardVector * Game.LastFrameTime * _movementSpeed;
            else if (CitizenFX.Core.Game.IsControlPressed(0, Control.VehicleFlyThrottleDown))
                _movementSpeed = 10f;
            //_cam.Position += _cam.ForwardVector * Game.LastFrameTime * _movementSpeed;


            // Forward and Back
            if (CitizenFX.Core.Game.IsControlPressed(0, Control.MoveUpOnly))
                    _cam.Position += _cam.UpVector * CitizenFX.Core.Game.LastFrameTime * _movementSpeed;
                else if (CitizenFX.Core.Game.IsControlPressed(0, Control.MoveDown))
                    _cam.Position -= _cam.UpVector * CitizenFX.Core.Game.LastFrameTime * _movementSpeed;

            // Left and Right
            if (CitizenFX.Core.Game.IsControlPressed(0, Control.MoveRight))
                _cam.Position += _cam.RightVector * CitizenFX.Core.Game.LastFrameTime * _movementSpeed;
            else if (CitizenFX.Core.Game.IsControlPressed(0, Control.MoveLeftOnly))
                _cam.Position -= _cam.RightVector * CitizenFX.Core.Game.LastFrameTime * _movementSpeed;
        }
    }
}
