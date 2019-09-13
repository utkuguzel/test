/*global
    piranha
*/

Vue.component("call-to-action-block", {
    props: ["uid", "toolbar", "model"],
    data: function () {
        return {
            column: this.model.column.value
        };
    },
    methods: {
        onTitleBlur: function (e) {
            this.model.title.value = e.target.innerText;
        },

        onBlurCol: function (e) {
            this.model.column.value = e.target.innerHTML;
        }
    },
    computed: {
        isTitleEmpty: function () {
            return piranha.utils.isEmptyText(this.model.title.value);
        },

        isEmptyCol: function () {
            return piranha.utils.isEmptyHtml(this.model.column.value);
        }
    },
    mounted: function () {
        piranha.editor.addInline(this.uid + 1, this.toolbar);
    },
    beforeDestroy: function () {
        piranha.editor.remove(this.uid + 1);
    },
    template:
        `<div class='block block-body block-call-to-action'>
            <h3 contenteditable='true' v-html='model.title.value' v-on:blur='onTitleBlur' :class='{ empty: isTitleEmpty}'>Lorem ipsum dolor sit amet</h3>
        	<div class='row'>
        		<div class='col-md-12'>
        			<div class='block-content'>
                        <div :class='{ empty: isEmptyCol }'>
                            <div :id='uid + 1' contenteditable='true' spellcheck='false' v-html='column' v-on:blur='onBlurCol'></div>
                        </div>
                	</div>
                </div>
            </div>
        </div>`
});