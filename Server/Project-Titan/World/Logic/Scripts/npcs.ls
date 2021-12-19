entity_state(name: "Bank Goblin", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "Store your items in our Bank for safe keeping.", text: "It is our utmost responsibility to store your items.", text: "The Goblin Bank has secured treasures and trinkets for centuries!", periodMin: 100, periodMax: 180)
})
entity_state(name: "Deer", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 4, secMax: 8, true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "wander", actions: {
		wander_nexus(speed: 1, period: 1000)
		stay_on_tile(tile: "Plain Grass", speed: 1, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 2, secMax: 5, true: {
			set_state(name: "wait", parent: 1)
		})
	})
})
entity_state(name: "Bunny", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 2, secMax: 5, true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "wander", actions: {
		wander_nexus(speed: 2, period: 1000)
		stay_on_tile(tile: "Plain Grass", speed: 1, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 2, secMax: 5, true: {
			set_state(name: "wait", parent: 1)
		})
	})
})
entity_state(name: "Antique Vendor", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		stay_on_tag(tag: "Market", speed: 1.5, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "#player, I'm sure I have something you need.", text: "Jewels and gems, riches to behold.", periodMin: 60, periodMax: 90)
})
entity_state(name: "Emote Vendor", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		stay_on_tag(tag: "Market", speed: 1.5, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "Express yourself! Share with the world through our selection of emotes.", periodMin: 60, periodMax: 90)
})
entity_state(name: "Wardrobe Vendor", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		stay_on_tag(tag: "Market", speed: 1.5, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "Fashion at affordable prices!", text: "The lastest styles, in stock now!", periodMin: 60, periodMax: 90)
})
entity_state(name: "Companion Vendor", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		stay_on_tag(tag: "Market", speed: 1.5, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "Beasts of burden available here! Store extra items on the go.", text: "#player you look in need of storage. Meet your companion today!", periodMin: 60, periodMax: 90)
})
entity_state(name: "Package Vendor", actions: {
	state(name: "wait", actions: {
		if_elapsed(secMin: 5, secMax: 10, true: {
			set_state(name: "walk", parent: 1)
		})
	})
	state(name: "walk", actions: {
		wander_nexus(speed: 0.5, period: 1000)
		stay_on_tag(tag: "Market", speed: 1.5, distance: 0, period: 0.01, enforce: 2)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "wait", parent: 1)
		})
	})
	taunt(text: "Great deals, all in package pricing!", text: "Start off right with my beginner's pack!", periodMin: 60, periodMax: 90)
})
entity_state(name: "Market Mule", actions: {
	state(name: "still", actions: {
		if_elapsed(sec: 1, true: {
			set_texture(index: 0)
		})
		if_elapsed(sec: 5, true: {
			set_texture(index: 2)
			set_state(name: "rest", parent: 1)
		})
	})
	state(name: "rest", actions: {
		if_elapsed(sec: 1, true: {
			set_texture(index: 1)
		})
		if_elapsed(secMin: 15, secMax: 45, true: {
			set_texture(index: 3)
			set_state(name: "still", parent: 1)
		})
	})
})
entity_state(name: "Fireside Man", actions: {
	state(name: "still", actions: {
		set_texture(index: 0)
		if_elapsed(secMin: 10, secMax: 16, true: {
			set_state(name: "look", parent: 1)
		})
	})
	state(name: "look", actions: {
		set_texture(index: 1)
		if_elapsed(sec: 1.4, true: {
			set_state(name: "still", parent: 1)
		})
	})
	taunt(text: "You'll find it to be easier here on the beach.", text: "<i>~ Dum dumdiddy dum Dodiddy dum du dun.. ~</i>", text: "Beware of bandits.. they carry better armor and weapons.", periodMin: 16, periodMax: 34)
})
entity_state(name: "Blacksmith", actions: {
	state(name: "move", actions: {
		set_texture(index: 0)
		move_to_region(speed: 4, region: "Blacksmith")
		if_elapsed(sec: 2, true: {
			set_state(name: "hammer", parent: 1)
		})
	})
	state(name: "hammer", actions: {
		set_texture(index: 1)
		if_elapsed(secMin: 8, secMax: 12, true: {
			set_state(name: "move", parent: 1)
		})
	})
})
entity_state(name: "Forge Miner", actions: {
	state(name: "still", actions: {
		set_texture(index: 0)
		if_elapsed(secMin: 4, secMax: 8, true: {
			set_state(name: "mine", parent: 1)
		})
	})
	state(name: "mine", actions: {
		set_texture(index: 1)
		if_elapsed(secMin: 12, secMax: 16, true: {
			set_state(name: "still", parent: 1)
		})
	})
})
entity_state(name: "Giant Forge Miner", actions: {
	state(name: "still", actions: {
		set_texture(index: 0)
		if_elapsed(secMin: 4, secMax: 8, true: {
			set_state(name: "mine", parent: 1)
		})
	})
	state(name: "mine", actions: {
		set_texture(index: 1)
		if_elapsed(secMin: 12, secMax: 16, true: {
			set_state(name: "still", parent: 1)
		})
	})
})
entity_state(name: "Bubra Female Villager", actions: {
	state(name: "still", actions: {
		if_elapsed(secMin: 2, secMax: 8, true: {
			set_state(name: "move", parent: 1)
		})
	})
	state(name: "move", actions: {
		wander_nexus(speed: 1, period: 1000)
		stay_near_spawn(speed: 2, distance: 7, enforce: 4, period: 3)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "still", parent: 1)
		})
	})
})
entity_state(name: "Bubra Male Villager", actions: {
	state(name: "still", actions: {
		if_elapsed(secMin: 2, secMax: 8, true: {
			set_state(name: "move", parent: 1)
		})
	})
	state(name: "move", actions: {
		wander_nexus(speed: 1, period: 1000)
		stay_near_spawn(speed: 2, distance: 7, enforce: 4, period: 3)
		if_elapsed(secMin: 1.5, secMax: 3, true: {
			set_state(name: "still", parent: 1)
		})
	})
})