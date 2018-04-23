﻿namespace shader_test

open Global
open System
open System.Linq
open System.Collections.Generic

type Custom_Post_Effect(obj, objects_data) =
    inherit asd.PostEffect()

    let obj : Player = obj
    let objects_data : Shader_Objects = objects_data

    let shader2d_shade = @"
precision mediump float;
uniform sampler2D g_texture;
uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;
uniform vec2 rect_points_0;
uniform vec2 rect_points_1;
uniform vec2 rect_points_2;
uniform vec2 rect_points_3;

in vec4 inPosition;
in vec2 inUV;
in vec4 inColor;

out vec4 outOutput;

// const float hoge = 1.0;

vec2 normed_inPos() {
    return inPosition.xy * resolution / min(resolution.x, resolution.y);
}

vec2 normalize(vec2 p) {
    return vec2(p.x / resolution.x * 2.0 - 1.0, -p.y / resolution.y * 2.0 + 1.0) * resolution / min(resolution.x, resolution.y);
}

float light(vec2 s, float b) {
    vec2 p = normed_inPos();
    return b / length(p - s);
}

bool in_shadow(vec2 ls, vec2 v1, vec2 v2) {
    vec2 p = normed_inPos();
    vec2 A = v2 - v1;
    vec2 B = p - ls;
    vec2 C = vec2(A.y, -A.x);
    vec2 D = vec2(B.y, -B.x);

    float dist = dot(v1 - ls, C) / dot(B, C);
    float range = dot(ls - v1, D) / dot(A, D);

    return (0.0 < dist && dist < 1.0 && 0.0 < range && range < 1.0);
}

void main() {
    vec2 origin = vec2(0.0, 0.0);
    vec2 p = normed_inPos();
    vec2 m = normalize(mouse);
    const int rect_vn = 4;
    // multi line array is not supported

    vec2 rect_points_norm[rect_vn];

    rect_points_norm[0] = normalize(rect_points_0);
    rect_points_norm[1] = normalize(rect_points_1);
    rect_points_norm[2] = normalize(rect_points_2);
    rect_points_norm[3] = normalize(rect_points_3);

    float brightness = 0.0;

    vec2 q = origin + vec2(cos(0.0), sin(0.0)) * 0.6;

    bool lightened_m = true, lightened_q = true, in_rect = true;

    for(int i=0; i < rect_vn; i++) {
        lightened_m = lightened_m && !in_shadow(m, rect_points_norm[i], rect_points_norm[int(mod((i+1), rect_vn))]);
        lightened_q = lightened_q && !in_shadow(q, rect_points_norm[i], rect_points_norm[int(mod((i+1), rect_vn))]);
        in_rect = in_rect && !in_shadow((rect_points_norm[0] + rect_points_norm[2]) / 2.0, rect_points_norm[i], rect_points_norm[int(mod((i+1), rect_vn))]);
    }

    if(lightened_m || in_rect) {
        brightness += light(m, 0.08);
    }

    if(lightened_q || in_rect) {
        brightness += light(q, 0.1);
    }

    float a = 0.5;

    float alpha = 0.0;

    if(!(lightened_m || lightened_q)) {
        if(in_rect) {
            alpha = min(brightness / 5.0, texture(g_texture, inUV.xy).w);
        } else { // not lightened
            alpha = min(0.005 + length(p - m) / 40.0, brightness * 0.9);
        }
    } else {
        alpha = brightness;
    }

    vec4 color = vec4(texture(g_texture, inUV.xy).xyz * (1.0 - a) + alpha * a, alpha);

    outOutput = color;
}
"

    let mutable shader = asd.Engine.Graphics.CreateShader2D(shader2d_shade)

    let mutable material2d = asd.Engine.Graphics.CreateMaterial2D(shader)

    let mutable count = 0.0f

    (* let light_shadow_define_gl = 
        String.Format("#define RESOLUTION vec2({0:.0}, {1:.0})\n", Width, Height) +
        String.Format("#define COUNT {0:.0}\n", count) +
        String.Format("#define MOUSE vec2({0:.0}, {1:.0})\n", asd.Engine.Mouse.Position.X, asd.Engine.Mouse.Position.Y)
    *)

    member private this.generate_shader_code () =
        let light_shadow_head_gl = @"
precision mediump float;
uniform sampler2D g_texture;
uniform vec2 mouse;
uniform vec2 resolution;

in vec4 inPosition;
in vec2 inUV;
in vec4 inColor;

out vec4 outOutput;

const vec2 normalized_inPosition = inPosition.xy * resolution / min(resolution.x, resolution.y);

vec2 normalize(vec2 p) {
    return vec2(p.x / resolution.x * 2.0 - 1.0, -p.y / resolution.y * 2.0 + 1.0) * resolution / min(resolution.x, resolution.y);
}

float normalize_radius(vec2 center, float radius) {
    return length(normalize(center) - normalize(center + vec2(radius, 0.0)));
}

float light(vec2 s, float b) {
    vec2 p = normalized_inPosition;
    return b / length(p - s);
}

bool in_shadow_line(vec2 ls, vec2 v1, vec2 v2) {
    vec2 p = normalized_inPosition;
    vec2 A = v2 - v1;
    vec2 B = p - ls;
    vec2 C = vec2(A.y, -A.x);
    vec2 D = vec2(B.y, -B.x);

    float dist = dot(v1 - ls, C) / dot(B, C);
    float range = dot(ls - v1, D) / dot(A, D);

    return (0.0 < dist && dist < 1.0 && 0.0 < range && range < 1.0);
}

bool in_shadow_circle(vec2 ls, vec2 center, float radius) {
    vec2 p = normalized_inPosition;
    if(length(ls - pos) < radius) {
        return (length(p - pos) > radius);
    } else {

    }

}

bool in_circle(vec2 ls, vec2 center, float radius) {

}

void main() {
    vec2 origin = vec2(0.0, 0.0);
    vec2 p = normalized_inPosition;
    float brightness = 0.0;
    bool lightened_m = true, inside_object = true;

"
        let light_shadow_generated_head_gl =

            let join_rect_uniform =
                let rectobj_num = objects_data.Rectangle_Objects.Count
                let rect_uniform_join rectobj_i =
                    let vertex_i = 4
                    [0..vertex_i-1].Select(fun x -> String.Format("uniform vec2 rectobj_{}_{};", rectobj_i, x)) |> String.concat("\n")
                [0..rectobj_num-1].Select(rect_uniform_join) |> String.concat("")
            
            let join_vertex_uniform =
                let vobj_num = objects_data.Vertex_Objects.Count
                let vertex_uniform_join vobj_i =
                    let vertex_i = (objects_data.Vertex_Objects.[vobj_i]).Vertex_List.Count
                    [0..vertex_i-1].Select(fun x -> String.Format("uniform vec2 vobj_{}_{};", vobj_i, x)) |> String.concat("\n")
                [0..vobj_num-1].Select(vertex_uniform_join) |> String.concat("")
               
            let join_circle_uniform =
                let circle_num = objects_data.circle_Objects.Count
                let circle_uniform_join circle_i =
                    let light_obj = objects_data.circle_Objects.[circle_i]
                    String.Format("uniform vec2 circle_pos_{0};\nuniform float circle_radius_{0};\n", circle_i)
                [0..circle_num-1].Select(circle_uniform_join) |> String.concat("")
            
            let join_light_uniform =
                let light_num = objects_data.Light_Objects.Count
                let light_uniform_join light_i =
                    let light_obj = objects_data.Light_Objects.[light_i]
                    String.Format("uniform vec2 light_pos_{0};\nuniform float light_brightness_{0};\n", light_i)
                [0..light_num-1].Select(light_uniform_join) |> String.concat("")
            
            
            join_rect_uniform +
            join_vertex_uniform + 
            join_light_uniform


        let light_shadow_generated_main_gl =

            let rectobj_num = objects_data.Rectangle_Objects.Count
            let join_rectangle_obj =
                let vertex_i = 4
                let rect_obj_join (rectobj_i : int) =
                    String.Format("        vec2 rectobj_{0}[{1}] = vec2[]({2});\n", 
                                  rectobj_i, 
                                  vertex_i, 
                                  [0..vertex_i-1].Select(fun x -> String.Format("normalize(rectobj_{0}_{1})", rectobj_i, x)) |> String.concat (", "))
                String.Format("        int rectobj_num = {0};", rectobj_num) +
                ([0..rectobj_num-1].Select(rect_obj_join) |> String.concat(""))
            
            
            let vobj_num = objects_data.Vertex_Objects.Count
            let join_vertex_obj =

                let fvertex_i x = (objects_data.Vertex_Objects.[x]).Vertex_List.Count
                let vertex_obj_join (vobj_i : int) =
                    let vertex_i = fvertex_i vobj_i

                    String.Format("        vec2 vobj_{}[{}] = vec2[]({});\n", 
                                  vobj_i, 
                                  vertex_i, 
                                  [0..vertex_i-1].Select(fun x -> String.Format("normalize(vobj_{}_{})", vobj_i, x)) |> String.concat (", "))
                
                String.Format("        int vobj_num = {};", vobj_num) +
                String.Format("        int vertex_num[{}] = int[]({});\n", vobj_num, [0..vobj_num-1].Select(fun x -> String.Format("{}", fvertex_i x)) |> String.concat(", ")) +
                ([0..vobj_num-1].Select(vertex_obj_join) |> String.concat(""))
            
            
            let circleobj_num = objects_data.circle_Objects.Count
            let join_circle_obj =
                String.Format("        int circle_num = {};", circleobj_num) +
                String.Format("        vec2 circle_pos[circle_num] = vec2[]({});\n", [0..circleobj_num-1].Select(fun x -> String.Format("normalize(circle_pos_{})", x)) |> String.concat ", ") +
                String.Format("        float circle_radius[circle_num] = float[]({});\n", [0..circleobj_num-1].Select(fun x -> String.Format("circle_radius_{}", x)) |> String.concat ", ") 
            
            
            let light_num = objects_data.Light_Objects.Count
            let join_light_obj =
                String.Format("        int light_num = {};\n", light_num) +
                String.Format("        bool lightened[{light_num] = bool[]({});\n", [0..light_num-1].Select(fun x -> "true") |> String.concat ", ") +
                String.Format("        vec2 light_pos[light_num] = vec2[]({});\n", [0..light_num-1].Select(fun x -> String.Format("normalize(light_pos_{})", x)) |> String.concat ", ") +
                String.Format("        float light_brightness[light_num] = float[]({});\n", [0..light_num-1].Select(fun x -> String.Format("light_brightness_{}", x)) |> String.concat ", ")



            
            let main_loop =
                let loop_rect =
                    [0..rectobj_num-1]
                        |> List.map (fun rectobj_i -> 
                                (
                                    "        for(int rect_i=0; rect_i < 4; rect_i++) {\n" +
                                    String.Format("                lightened_m = lightened_m && !in_shadow(m, rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), 4))]);", rectobj_i) + @"
                    for(int light_i=0; light_i < light_num; light_i++) {
                        if(lightened[light_i]) {
" + 
                                    String.Format("                            lightened[light_i] = lightened[light_i] && !in_shadow(q, rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), rect_i))]);", rectobj_i) + @"
                        }
                    }
                    inside_object = inside_object && !in_shadow( (sum(rectobj_{0}[0] + rectobj_{0}[2]) ) / float(2), rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), rect_i))]);
                }
                "
                                )
                            )
                        |> List.fold (fun x y -> x + y) ""
                
                let loop_vertex =
                    [0..vobj_num]
                        |> List.map (fun vobj_i ->
                                (
                                    String.Format("        for(int vertex_i=0; vertex_i < vertex_num[{0}]; vertex_i++) {\n", vobj_i) +
                                    String.Format("                lightened_m = lightened_m && !in_shadow(m, vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_num[{0}]))]);\n", vobj_i) + @"
                    for(int light_i=0; light_i < light_num; light_i++) {
                        if(lightened[light_i]) {
" + 
                                    String.Format("                            lightened[light_i] = lightened[light_i] && !in_shadow(q, vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_i))]);") + @"
                        }
                    }
                    inside_object = inside_object && !in_shadow(vobj_{0}[0] + vobj_{0}[1] + vobj_{0}[2] ) / 3.0, vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_i))]);
                }
                "
                                )
                            )
                        |> List.fold (fun x y -> x + y) ""
                
                loop_rect

            join_rectangle_obj + join_vertex_obj + join_circle_obj + join_light_obj + main_loop
                |> ignore

            @"
    
        /*
        int vobj_num = 2; // example done
        int vertex_i = 3; // example done
        vec2 vobj_{vobj_i}[vertex_i] = vec2[](vec2(0.2, 0.2), vec2(0.2, 0.4), vec2(-0.4, 0.7)); // example done


        int light_num = 3; // example done
        bool lightened[light_num] = bool[](true, true, true); // example done
        vec2 light_pos[light_num] = vec2[](vec2(0.0, 0.0), vec2(0.2, 0.4), vec2(-0.4, 0.7)); // example done
        float light_brightness[light_num] = float[](0.08, 0.05, 0.1); // example done

        int vobj_{2}_n = 3;
        vec2 vobj_{2}[vobj_{2}_n] = vec2[](vec2(0.0, 0.0), vec2(1.0, 0.0), vec2(0.0, 1.0));


        for(int vobj_i = 0; vobj_i < vobj_num; vobj_i++) { // ここを展開する
            for(int vertex_i=0; vertex_i < vobj_{vobj_i}_n; vertex_i++) {
                vobj_{vobj_i}[vertex_i]
                lightened_m = lightened_m && !in_shadow(m, rect_points_norm[i], rect_points_norm[int(mod((i+1), vertex_i))]);
                for(int light_i=0; light_i < light_num; light_i++) {
                    if(lightened[light_i]) {
                        lightened[light_i] = lightened[light_i] && !in_shadow(q, rect_points_norm[i], rect_points_norm[int(mod((i+1), vertex_i))]);
                    }
                }

                inside_object = inside_object && !in_shadow( (sum(rect_points_norm[]) ) / float(vertex_i), rect_points_norm[i], rect_points_norm[int(mod((i+1), vertex_i))]);
            }
        }
        */
"

        let light_shadow_foot_gl = @"
        
if(lightened_m || in_rect) {
    brightness += light(m, 0.08);
}

for(int light_i=0; light_i < light_num; light_i++) {
    if(lightened[light_i]|| inside_object) {
        brightness += light(light_pos[light_i], light_brightness[light_i]);
    }
}

bool lightened_or = true;

for(int light_i=0; light_i < light_num; light_i++) {
    lightened_or lightened_or && lightened[light_i];
}

float alpha = 0.0;
float a = 0.5;

if(!(lightened_or)) { // if not lightened
    if(in_rect) {
        alpha = min(brightness / 5.0, texture(g_texture, inUV.xy).w);
    } else { // in the shadow
        alpha = min(0.005 + length(p - m) / 40.0, brightness * 0.9);
    }
} else { // lightened
    alpha = brightness;
}

vec4 color = vec4(texture(g_texture, inUV.xy).xyz * (1.0 - a) + alpha * a, alpha);

outOutput = color;
}
"

        let result =
            light_shadow_generated_head_gl + 
            light_shadow_head_gl + 
            light_shadow_generated_main_gl + 
            light_shadow_foot_gl
        System.Console.WriteLine result

        result

    override this.OnDraw(dst, src) =
        count <- count + 1.0f / 60.0f

        material2d.SetTexture2D("g_texture", src)
        material2d.SetFloat("time", count)
        material2d.SetVector2DF("mouse", asd.Engine.Mouse.Position)
        material2d.SetVector2DF("resolution", WindowSize)


        material2d.SetVector2DF("rect_points_0", obj.get_point_pos 0)
        material2d.SetVector2DF("rect_points_1", obj.get_point_pos 1)
        material2d.SetVector2DF("rect_points_2", obj.get_point_pos 2)
        material2d.SetVector2DF("rect_points_3", obj.get_point_pos 3)


        if objects_data.Updated_State then
            shader <- asd.Engine.Graphics.CreateShader2D(this.generate_shader_code())
            material2d <- asd.Engine.Graphics.CreateMaterial2D(shader)

            for index in[0..objects_data.Rectangle_Objects.Count-1] do
                material2d.SetVector2DF(String.Format("rectobj_{0}_0", index), (objects_data.Rectangle_Objects.[index]))


        this.DrawOnTexture2DWithMaterial(dst, material2d)