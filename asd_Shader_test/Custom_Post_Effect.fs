namespace shader_test

open Global
open System
open System.Linq
open System.Collections.Generic

type Custom_Post_Effect(objects_data) =
    inherit asd.PostEffect()

    let objects_data : Shader_Objects = objects_data

    [<DefaultValue>]val mutable shader : asd.Shader2D
    [<DefaultValue>]val mutable material2d : asd.Material2D

    let mutable count = 0.0f

    member this.generate_shader_code () =
        let light_shadow_head_gl = @"
precision mediump float;
uniform sampler2D g_texture;
uniform vec2 mouse;
uniform vec2 resolution;

in vec4 inPosition;
in vec2 inUV;
in vec4 inColor;

out vec4 outOutput;

vec2 normalized_inPosition() {
    return inPosition.xy * resolution / min(resolution.x, resolution.y);
}

vec2 normalize(vec2 p) {
    return vec2(p.x / resolution.x * 2.0 - 1.0, -p.y / resolution.y * 2.0 + 1.0) * resolution / min(resolution.x, resolution.y);
}

float normalize_radius(vec2 center, float radius) {
    return length(normalize(vec2(radius, 0.0)));
}

float light(vec2 s, float b) {
    vec2 p = normalized_inPosition();
    return b / length(p - s);
}

bool in_shadow_line(vec2 ls, vec2 v1, vec2 v2) {
    vec2 p = normalized_inPosition();
    vec2 A = v2 - v1;
    vec2 B = p - ls;
    vec2 C = vec2(A.y, -A.x);
    vec2 D = vec2(B.y, -B.x);

    float dist = dot(v1 - ls, C) / dot(B, C);
    float range = dot(ls - v1, D) / dot(A, D);

    return (0.0 < dist && dist < 1.0 && 0.0 < range && range < 1.0);
}

bool in_circle(vec2 center, float radius) {
    vec2 p = normalized_inPosition();
    return (length(p - center) < radius);
}

bool in_shadow_circle(vec2 ls, vec2 center, float radius) {
    vec2 p = normalized_inPosition();
    if(length(ls - center) < radius) {
        return (length(p - center) > radius);
    } else {
        float z = length(ls - center);
        float cos = radius / z;
        float sin = sqrt(z * z - radius * radius) / z;
        vec2 c_to_ls = ls - center;

        vec2 pe = c_to_ls / z * radius * cos;
        vec2 ve = vec2(c_to_ls.y, -c_to_ls.x) / z * radius * sin;

        return (in_shadow_line(ls, center + pe + ve, center + pe - ve) || in_circle(center, radius));
    }

}



void main() {
    vec2 origin = vec2(0.0, 0.0);
    vec2 p = normalized_inPosition();
    vec2 m = normalize(mouse);
    float brightness = 0.0;
    bool lightened_m = true, inside_object = true;

"
        let rectobj_num = objects_data.Rectangle_Objects.Count
        let vobj_num = objects_data.Vertex_Objects.Count
        let circleobj_num = objects_data.Circle_Objects.Count
        let light_num = objects_data.Light_Objects.Count


        let light_shadow_generated_head_gl =

            let join_rect_uniform =
                let rect_uniform_join rectobj_i =
                    let vertex_i = 4
                    [0..vertex_i-1].Select(fun x -> String.Format("uniform vec2 rectobj_{0}_{1};\n", rectobj_i, x)) |> String.concat ""
                [0..rectobj_num-1].Select(rect_uniform_join) |> String.concat ""
            
            let join_vertex_uniform =
                let vertex_uniform_join vobj_i =
                    let vertex_i = (objects_data.Vertex_Objects.[vobj_i]).Vertex_List.Count
                    [0..vertex_i-1].Select(fun x -> String.Format("uniform vec2 vobj_{0}_{1};\n", vobj_i, x)) |> String.concat ""
                [0..vobj_num-1].Select(vertex_uniform_join) |> String.concat ""
               
            let join_circle_uniform =
                let circle_uniform_join circle_i =
                    let light_obj = objects_data.Circle_Objects.[circle_i]
                    String.Format("uniform vec2 circle_pos_{0};\nuniform float circle_radius_{0};\n", circle_i)
                [0..circleobj_num-1].Select(circle_uniform_join) |> String.concat ""
            
            let join_light_uniform =
                let light_uniform_join light_i =
                    let light_obj = objects_data.Light_Objects.[light_i]
                    String.Format("uniform vec2 light_pos_{0};\nuniform float light_brightness_{0};\n", light_i)
                [0..light_num-1].Select(light_uniform_join) |> String.concat ""
            
            
            join_rect_uniform + join_vertex_uniform + join_circle_uniform + join_light_uniform


        let light_shadow_generated_main_gl =

            let join_rectangle_obj =
                let vertex_i = 4
                let rect_obj_join (rectobj_i : int) =
                    String.Format("    vec2 rectobj_{0}[{1}] = vec2[]({2});\n", 
                                  rectobj_i, 
                                  vertex_i, 
                                  [0..vertex_i-1].Select(fun x -> String.Format("normalize(rectobj_{0}_{1})", rectobj_i, x)) |> String.concat ", ")
                String.Format("    int rectobj_num = {0};\n", rectobj_num) +
                ([0..rectobj_num-1].Select(rect_obj_join) |> String.concat "")
            
            
            let join_vertex_obj =
                let fvertex_i x = (objects_data.Vertex_Objects.[x]).Vertex_List.Count
                let vertex_obj_join (vobj_i : int) =

                    let vertex_i = fvertex_i vobj_i
                    String.Format("    vec2 vobj_{0}[{1}] = vec2[]({2});\n", 
                                  vobj_i, 
                                  vertex_i, 
                                  [0..vertex_i-1].Select(fun x -> String.Format("normalize(vobj_{0}_{1})", vobj_i, x)) |> String.concat ", ")

                String.Format("    int vobj_num = {0};\n", vobj_num) +
                if vobj_num > 0 then
                    String.Format("    int vertex_num[{0}] = int[]({1});\n", vobj_num, [0..vobj_num-1].Select(fun x -> String.Format("{0}", fvertex_i x)) |> String.concat ", ") +
                    ([0..vobj_num-1].Select(vertex_obj_join) |> String.concat "")
                else ""
            
            
            let join_circle_obj =
                String.Format("    int circle_num = {0};\n", circleobj_num) +
                if circleobj_num > 0 then
                    String.Format("    vec2 circle_pos[{0}] = vec2[]({1});\n", circleobj_num, [0..circleobj_num-1].Select(fun x -> String.Format("normalize(circle_pos_{0})", x)) |> String.concat ", ") +
                    String.Format("    float circle_radius[{0}] = float[]({1});\n", circleobj_num, [0..circleobj_num-1].Select(fun x -> String.Format("normalize_radius(circle_pos_{0}, circle_radius_{0})", x)) |> String.concat ", ") 
                else ""
            
            let join_light_obj =
                String.Format("    int light_num = {0};\n", light_num) +
                if light_num > 0 then
                    String.Format("    bool lightened[{0}] = bool[]({1});\n", light_num, [0..light_num-1].Select(fun x -> "true") |> String.concat ", ") +
                    String.Format("    vec2 light_pos[{0}] = vec2[]({1});\n", light_num, [0..light_num-1].Select(fun x -> String.Format("normalize(light_pos_{0})", x)) |> String.concat ", ") +
                    String.Format("    float light_brightness[{0}] = float[]({1});\n", light_num, [0..light_num-1].Select(fun x -> String.Format("light_brightness_{0}", x)) |> String.concat ", ")
                else ""


            
            let main_loop =
                let loop_rect =
                    if rectobj_num > 0 then
                        [0..rectobj_num-1]
                            |> List.map (fun rectobj_i -> 
                                    (
                                        "    for(int rect_i=0; rect_i < 4; rect_i++) {\n" +
                                        String.Format("        lightened_m = lightened_m && !in_shadow_line(m, rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), 4))]);\n", rectobj_i) + 

                                        if light_num > 0 then
                                            "        for(int light_i=0; light_i < light_num; light_i++) {\n" +
                                            "            if(lightened[light_i]) {\n" + 
                                            String.Format("                lightened[light_i] = lightened[light_i] && !in_shadow_line(light_pos[light_i], rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), rect_i))]);\n", rectobj_i) + 
                                            "            }\n" +
                                            "        }\n"
                                        else ""
                                        + String.Format("        inside_object = inside_object && !in_shadow_line((rectobj_{0}[0] + rectobj_{0}[2] ) / 2.0, rectobj_{0}[rect_i], rectobj_{0}[int(mod((rect_i+1), rect_i))]);\n", rectobj_i) +
                                        "    }\n"
                                    )
                                )
                            |> List.fold (fun x y -> x + y) ""
                    else ""
                
                let loop_vertex =
                    if vobj_num > 0 then
                        [0..vobj_num-1]
                            |> List.map (fun vobj_i ->
                                    (
                                        String.Format("        for(int vertex_i=0; vertex_i < vertex_num[{0}]; vertex_i++) {\n", vobj_i) +
                                        String.Format("            lightened_m = lightened_m && !in_shadow_line(m, vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_num[{0}]))]);\n", vobj_i) + 
                                        if light_num > 0 then
                                            "            for(int light_i=0; light_i < light_num; light_i++) {\n" +
                                            "                if(light_num > 0 && lightened[light_i]) {\n"+ 
                                            String.Format("                    lightened[light_i] = lightened[light_i] && !in_shadow_line(light_pos[light_i], vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_i))]);") + 
                                            "                }\n"
                                        else ""
                                        + "            }\n" +
                                        String.Format("            inside_object = inside_object && !in_shadow_line((vobj_{0}[0] + vobj_{0}[1] + vobj_{0}[2]) / 3.0, vobj_{0}[vertex_i], vobj_{0}[int(mod((vertex_i+1), vertex_i))]);\n", vobj_i) +
                                        "        }\n"
                                    )
                                )
                            |> List.fold (fun x y -> x + y) ""
                    else ""
                
                let loop_circle = 
                    if circleobj_num > 0 then 
                        @"
    for(int circle_i=0; circle_i < circle_num; circle_i++) {
        lightened_m = lightened_m && !in_shadow_circle(ls, circle_pos[circle_i], circle_radius[circle_i]);;" 
                        + 
                        if light_num > 0 then
                            @"
    for(int light_i=0; light_i < light_num; light_i++) {
        if(light_num > 0 && lightened[light_i]) {
            lightened[light_i] = lightened[light_i] && !in_shadow_circle(ls, circle_pos[circle_i], circle_radius[circle_i]);
        }"
                        else ""
                        + @"
            inside_object = inside_object && !in_circle(circle_pos[circle_i], circle_radius[circle_i]);
        }
    }
"
                    else ""


                
                loop_rect + loop_vertex

            join_rectangle_obj + join_vertex_obj + join_circle_obj + join_light_obj + main_loop
        
        
        let light_shadow_generated_foot_gl = 
            @"
    bool lightened_or = false;

    if(lightened_m || inside_object) {
        brightness += light(m, 0.08);
    }
    lightened_or = lightened_or ||  lightened_m;
"
            +

            if light_num > 0 then @"
    for(int light_i=0; light_i < light_num; light_i++) {
        lightened_or = lightened_or || lightened[light_i];
    }

    for(int light_i=0; light_i < light_num; light_i++) { 
        if(lightened[light_i]|| inside_object) { 
            brightness += light(light_pos[light_i], light_brightness[light_i]); 
        } 
    } 
"
            else ""

            + @"
    
    float alpha = 0.0;
    float a = 0.5;
    
    if(!lightened_or) { // if not lightened
        if(inside_object) {
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
            light_shadow_generated_foot_gl
        System.Console.WriteLine result

        result

    override this.OnDraw(dst, src) =
        count <- count + 1.0f / 60.0f

        if objects_data.Updated_State then
            this.shader <- asd.Engine.Graphics.CreateShader2D(this.generate_shader_code())
            this.material2d <- asd.Engine.Graphics.CreateMaterial2D(this.shader)

            objects_data.Updated_State <- false

        this.material2d.SetTexture2D("g_texture", src)
        this.material2d.SetFloat("time", count)
        this.material2d.SetVector2DF("mouse", asd.Engine.Mouse.Position)
        this.material2d.SetVector2DF("resolution", WindowSize)

        for index in [0..objects_data.Rectangle_Objects.Count-1] do
            let rectobj = objects_data.Rectangle_Objects.[index] in
            for index_r in [0..4-1] do
                this.material2d.SetVector2DF(String.Format("rectobj_{0}_{1}", index, index_r), ((rectobj :> VertexInterface).vertex_pos index_r))
        
        for index in [0..objects_data.Vertex_Objects.Count-1] do
            let vobj = objects_data.Vertex_Objects.[index] in
            for index_r in [0..vobj.Vertex_List.Count-1] do
                this.material2d.SetVector2DF(String.Format("vobj_{0}_{1}", index, index_r), ((vobj :> VertexInterface).vertex_pos index_r))
        
        for index in [0..objects_data.Circle_Objects.Count-1] do
            let circleobj = objects_data.Circle_Objects.[index]
            this.material2d.SetVector2DF(String.Format("circle_pos_{0}", index), circleobj.Position)
            this.material2d.SetFloat(String.Format("circle_radius_{0}", index), circleobj.Radius)
        
        for index in [0..objects_data.Light_Objects.Count-1] do
            let lightobj = objects_data.Light_Objects.[index]
            this.material2d.SetVector2DF(String.Format("light_pos_{0}", index), lightobj.Position)
            this.material2d.SetFloat(String.Format("light_brightness_{0}", index), lightobj.Brightness)
            

        this.DrawOnTexture2DWithMaterial(dst, this.material2d)