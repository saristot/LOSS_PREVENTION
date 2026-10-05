<template>
  <div class="block-root">
    <!-- Header -->
    <div class="block-header">
      <h3 class="font-bold">Image</h3>
      <div v-if="inDesignMode" class="actions">
        <button @click="$emit('edit', block)" class="text-blue-600 hover:underline">Edit</button>
        <button @click="$emit('delete', block)" class="text-red-600 hover:underline ml-3">Delete</button>
      </div>
    </div>

    <!-- Content -->
    <div class="block-content">
      <div class="image-wrap">
        <img
          v-if="block?.data?.url"
          :src="block.data.url"
          :alt="block.data.alt || 'image'"
          draggable="false"
        />
        <div v-else class="placeholder">
          No image selected
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  block: Object,
  inDesignMode: Boolean
});
</script>

<style scoped>
/* Root: full-size card with column layout */
.block-root {
  height: 100%;
  width: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  border: 1px solid #e5e7eb; /* tailwind gray-200 */
  border-radius: 0.375rem;   /* rounded-md */
  box-shadow: 0 1px 2px rgba(0,0,0,.06);
}

/* Header: fixed height, content won’t steal space */
.block-header {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: .5rem .75rem;     /* similar to p-2 */
  border-bottom: 1px solid #e5e7eb;
}

.actions { display: flex; align-items: center; }

/* Content: takes the rest of the height; min-h-0 prevents overflow in flex kids */
.block-content {
  flex: 1 1 auto;
  min-height: 0;             /* IMPORTANT with flex children + overflow */
  padding: .5rem;
  overflow: hidden;
}

/* Wrapper that actually defines the render box for the image */
.image-wrap {
  position: relative;
  width: 100%;
  height: 100%;
  overflow: hidden;          /* hide any subpixel overflow while resizing */
  border-radius: 0.25rem;
}

/* The image fills the available box but preserves aspect ratio */
.image-wrap > img {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: contain;       /* preserve aspect ratio within box */
  user-select: none;
  pointer-events: none;      /* avoids accidental drags while resizing grid */
  -webkit-user-drag: none;
  box-sizing: border-box;
}

/* Placeholder (when no URL) */
.placeholder {
  width: 100%;
  height: 100%;
  color: #6b7280;            /* gray-500 */
  display: grid;
  place-items: center;
  border: 1px dashed #e5e7eb;
  border-radius: 0.25rem;
}
</style>
