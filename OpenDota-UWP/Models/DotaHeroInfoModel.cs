﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDota_UWP.Models
{
    public class DotaHeroInfoModel
    {
        public Result result { get; set; }
    }

    public class Result
    {
        public Data data { get; set; }
        public int status { get; set; }
    }

    public class Data
    {
        public Hero[] heroes { get; set; }
    }

    public class Hero
    {
        public int id { get; set; }
        public string name { get; set; }
        public int order_id { get; set; }
        public string name_loc { get; set; }
        public string bio_loc { get; set; }
        public string hype_loc { get; set; }
        public string npe_desc_loc { get; set; }
        public double str_base { get; set; }
        public double str_gain { get; set; }
        public double agi_base { get; set; }
        public double agi_gain { get; set; }
        public double int_base { get; set; }
        public double int_gain { get; set; }
        public double primary_attr { get; set; }
        public double complexity { get; set; }
        public double attack_capability { get; set; }
        public double[] role_levels { get; set; }
        public double damage_min { get; set; }
        public double damage_max { get; set; }
        public double attack_rate { get; set; }
        public double attack_range { get; set; }
        public double projectile_speed { get; set; }
        public double armor { get; set; }
        public double magic_resistance { get; set; }
        public double movement_speed { get; set; }
        public double turn_rate { get; set; }
        public double sight_range_day { get; set; }
        public double sight_range_night { get; set; }
        public double max_health { get; set; }
        public double health_regen { get; set; }
        public double max_mana { get; set; }
        public double mana_regen { get; set; }
        public Ability[] abilities { get; set; }
        public Talent[] talents { get; set; }
    }

    public class Ability
    {
        public int id { get; set; }
        public string name { get; set; }
        public string name_loc { get; set; }
        public string desc_loc { get; set; }
        public string lore_loc { get; set; }
        public string[] notes_loc { get; set; }
        public string shard_loc { get; set; }
        public string scepter_loc { get; set; }
        public double type { get; set; }
        public string behavior { get; set; }
        public double target_team { get; set; }
        public double target_type { get; set; }
        public double flags { get; set; }
        public double damage { get; set; }
        public double immunity { get; set; }
        public double dispellable { get; set; }
        public double max_level { get; set; }
        public double[] cast_ranges { get; set; }
        public double[] cast_points { get; set; }
        public double[] channel_times { get; set; }
        public double[] cooldowns { get; set; }
        public double[] durations { get; set; }
        public double[] damages { get; set; }
        public double[] mana_costs { get; set; }
        public double[] gold_costs { get; set; }
        public Special_Values[] special_values { get; set; }
        public bool is_item { get; set; }
        public bool ability_has_scepter { get; set; }
        public bool ability_has_shard { get; set; }
        public bool ability_is_granted_by_scepter { get; set; }
        public bool ability_is_granted_by_shard { get; set; }
        public double item_cost { get; set; }
        public double item_initial_charges { get; set; }
        public double item_neutral_tier { get; set; }
        public double item_stock_max { get; set; }
        public double item_stock_time { get; set; }
        public double item_quality { get; set; }
    }

    public class Talent
    {
        public int id { get; set; }
        public string name { get; set; }
        public string name_loc { get; set; }
        public string desc_loc { get; set; }
        public string lore_loc { get; set; }
        public string[] notes_loc { get; set; }
        public string shard_loc { get; set; }
        public string scepter_loc { get; set; }
        public double type { get; set; }
        public string behavior { get; set; }
        public double target_team { get; set; }
        public double target_type { get; set; }
        public double flags { get; set; }
        public double damage { get; set; }
        public double immunity { get; set; }
        public double dispellable { get; set; }
        public double max_level { get; set; }
        public double[] cast_ranges { get; set; }
        public double[] cast_points { get; set; }
        public double[] channel_times { get; set; }
        public double[] cooldowns { get; set; }
        public double[] durations { get; set; }
        public double[] damages { get; set; }
        public double[] mana_costs { get; set; }
        public double[] gold_costs { get; set; }
        public Special_Values[] special_values { get; set; }
        public bool is_item { get; set; }
        public bool ability_has_scepter { get; set; }
        public bool ability_has_shard { get; set; }
        public bool ability_is_granted_by_scepter { get; set; }
        public bool ability_is_granted_by_shard { get; set; }
        public double item_cost { get; set; }
        public double item_initial_charges { get; set; }
        public double item_neutral_tier { get; set; }
        public double item_stock_max { get; set; }
        public double item_stock_time { get; set; }
        public double item_quality { get; set; }
    }

    public class Special_Values
    {
        public string name { get; set; }
        public double[] values_float { get; set; }
        public bool is_percentage { get; set; }
        public string heading_loc { get; set; }
        public object[] bonuses { get; set; }
    }
}